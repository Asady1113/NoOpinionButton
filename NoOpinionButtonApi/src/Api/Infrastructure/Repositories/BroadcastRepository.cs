using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Core.Domain.Ports;
using System.Text;

namespace Infrastructure.Repository;

/// <summary>
/// メッセージ配信リポジトリのWebSocket実装
/// </summary>
//  IDisposableを実装すると、そのオブジェクトの処理が終わったら必ず Dispose() を呼ぶ
public class BroadcastRepository : IBroadcastRepository, IDisposable
{
    // WebSocket接続の管理・操作を行うAWS SDKのクライアント
    private readonly AmazonApiGatewayManagementApiClient _managementApiClient;
    private readonly string? _webSocketApiEndpoint;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public BroadcastRepository()
    {
        // 環境変数からWebSocket APIエンドポイントを取得
        // Lambda関数は起動するときに「環境変数」という設定値を読み取れます。
        // その環境変数の中に、API GatewayのWebSocket管理APIのエンドポイントURLを文字列で設定している
        // こうすることで、PostToConnectionAsync 等のメソッド呼び出しが正しいAPI Gatewayの管理エンドポイントに対して送信されるようになります。
        _webSocketApiEndpoint = Environment.GetEnvironmentVariable("WEBSOCKET_API_ENDPOINT");
        
        if (string.IsNullOrEmpty(_webSocketApiEndpoint))
        {
            throw new InvalidOperationException("WEBSOCKET_API_ENDPOINT environment variable is not set");
        }

        // API Gateway Management APIクライアントを初期化
        // AmazonApiGatewayManagementApiConfig にエンドポイントURLをセットして設定オブジェクトを作る
        var config = new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = _webSocketApiEndpoint
        };
        // その設定を使って AmazonApiGatewayManagementApiClient のインスタンスを作成
        _managementApiClient = new AmazonApiGatewayManagementApiClient(config);
    }

    /// <inheritdoc/>
    public async Task<bool> BroadcastToConnectionAsync(string connectionId, string message)
    {
        try
        {
            // メッセージをバイト配列に変換
            // 送る文字列メッセージを、WebSocketが扱いやすいバイトデータ（UTF-8）に変換。
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            // PostToConnectionリクエストを作成
            // 「誰に送るか」の接続IDと、「何を送るか」のデータをセットしたリクエストを作成
            var postRequest = new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = new MemoryStream(messageBytes)
            };

            // WebSocket接続にメッセージを送信
            // API Gatewayの管理用エンドポイント（管理API）に対してHTTPリクエストを送る。
            // API GatewayはそのconnectionIdのWebSocket接続を特定し、そこにメッセージをリアルタイムで配信する（Lambdaはこれを経由しないと送れない。）
            await _managementApiClient.PostToConnectionAsync(postRequest);
            
            Console.WriteLine($"Successfully sent message to connection {connectionId}");
            return true;
        }
        catch (GoneException)
        {
            // 接続が切断済み - 正常なケース
            Console.WriteLine($"Connection {connectionId} is no longer available (disconnected)");
            return false;
        }
        catch (Exception ex)
        {
            // その他のエラー
            Console.WriteLine($"Failed to send message to connection {connectionId}: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> BroadcastToMultipleConnectionsAsync(IEnumerable<string> connectionIds, string message)
    {
        var connectionIdList = connectionIds.ToList();
        
        if (!connectionIdList.Any())
        {
            Console.WriteLine("No connections to broadcast to");
            return 0;
        }
        
        Console.WriteLine($"Broadcasting to {connectionIdList.Count} connections: {message}");
        
        // 並列で配信処理 - 個別エラーハンドリング付き
        var tasks = connectionIdList.Select(async connectionId =>
        {
            try
            {
                var success = await BroadcastToConnectionAsync(connectionId, message);
                return success ? 1 : 0;
            }
            catch (Exception ex)
            {
                // 個別接続エラーはログ出力のみ（全体処理は継続）
                Console.WriteLine($"Error broadcasting to connection {connectionId}: {ex.Message}");
                return 0;
            }
        });
        
        try
        {
            // 全ての送信タスクが完了するのを待つ（並列実行）。
            var results = await Task.WhenAll(tasks);
            // 結果（0か1の配列）の合計を取り、成功数を求める。
            var successCount = results.Sum();
            
            Console.WriteLine($"Broadcast completed: {successCount}/{connectionIdList.Count} successful");
            return successCount;
        }
        catch (Exception ex)
        {
            // 全体エラー（通常は発生しないが念のため）
            Console.WriteLine($"Critical error during batch broadcast: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// リソースの解放
    /// </summary>
    public void Dispose()
    {
        // クラスが持っているリソース（ここでは _managementApiClient）の Dispose() を呼んでクリーンアップ。
        _managementApiClient?.Dispose();
    }
}