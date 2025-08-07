using Core.Domain.Ports;

namespace Infrastructure.Repository;

/// <summary>
/// メッセージ配信リポジトリのWebSocket実装
/// </summary>
public class BroadcastRepository : IBroadcastRepository
{
    /// <inheritdoc/>
    public async Task<bool> BroadcastToConnectionAsync(string connectionId, string message)
    {
        // 実際のWebSocket送信は後で実装（現在はスタブ）
        // TODO: API Gateway Management APIを使用してWebSocketにメッセージを送信
        
        // 現在はログ出力のみ
        Console.WriteLine($"Broadcasting to connection {connectionId}: {message}");
        
        // 非同期処理をシミュレート
        await Task.Delay(1);
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> BroadcastToMultipleConnectionsAsync(IEnumerable<string> connectionIds, string message)
    {
        // 実際のWebSocket送信は後で実装（現在はスタブ）
        // TODO: API Gateway Management APIを使用して一括配信を効率的に実装
        
        var successCount = 0;
        var connectionIdList = connectionIds.ToList();
        
        Console.WriteLine($"Broadcasting to {connectionIdList.Count} connections: {message}");
        
        // 並列で配信処理（実装時はエラーハンドリングを追加）
        var tasks = connectionIdList.Select(async connectionId =>
        {
            try
            {
                // 個別配信を並列実行
                var success = await BroadcastToConnectionAsync(connectionId, message);
                return success ? 1 : 0;
            }
            catch
            {
                // エラー時は0を返す
                return 0;
            }
        });
        
        var results = await Task.WhenAll(tasks);
        successCount = results.Sum();
        
        Console.WriteLine($"Broadcast completed: {successCount}/{connectionIdList.Count} successful");
        
        return successCount;
    }
}