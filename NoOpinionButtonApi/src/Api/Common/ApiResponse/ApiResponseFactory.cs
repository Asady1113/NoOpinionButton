using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Common.ApiResponse;

public class ApiResponseFactory
{
    /// <summary>
    /// 正常終了レスポンスを生成します。
    /// 指定されたデータを JSON にシリアライズして 200 OK として返します。
    /// </summary>
    /// <typeparam name="T">レスポンスデータの型</typeparam>
    /// <param name="body">レスポンスとして返すデータ</param>
    /// <returns>APIGatewayProxyResponse オブジェクト（ステータスコード 200）</returns>
    public static APIGatewayProxyResponse Ok<T>(T body) =>
        BuildResponse(200, new ApiResponse<T> { Data = body });

    /// <summary>
    /// クライアントのリクエスト不正を示す 400 Bad Request レスポンスを生成します。
    /// エラーメッセージを JSON にシリアライズして返します。
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>APIGatewayProxyResponse オブジェクト（ステータスコード 400）</returns>
    public static APIGatewayProxyResponse BadRequest(string errorMessage) =>
        BuildResponse(400, new ApiResponse<object> { Error = errorMessage });

    /// <summary>
    /// 認証情報が正しくない／足りないことを示す401 Unauthorizedレスポンスを生成します。
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>APIGatewayProxyResponse オブジェクト（ステータスコード 401）</returns>
    public static APIGatewayProxyResponse Unauthorized(string errorMessage) =>
        BuildResponse(401, new ApiResponse<object> { Error = errorMessage });

    /// <summary>
    /// リクエストされたリソースが見つからないことを示す 404 Not Found レスポンスを生成します。
    /// 固定メッセージを JSON にシリアライズして返します。
    /// </summary>
    /// <returns>APIGatewayProxyResponse オブジェクト（ステータスコード 404）</returns>
    public static APIGatewayProxyResponse NotFound(string errorMessage) =>
        BuildResponse(404, new ApiResponse<object> { Error = errorMessage });

    /// <summary>
    /// サーバー内部エラーを示す 500 Internal Server Error レスポンスを生成します。
    /// 汎用的なエラーメッセージを JSON にシリアライズして返します。
    /// </summary>
    /// <returns>APIGatewayProxyResponse オブジェクト（ステータスコード 500）</returns>
    public static APIGatewayProxyResponse ServerError() =>
        BuildResponse(500, new ApiResponse<object> { Error = "Internal server error. Please try again later." });

    /// <summary>
    /// 指定されたステータスコードとレスポンスボディをもとに
    /// <c>APIGatewayProxyResponse</c> を生成します。
    /// レスポンスボディは JSON にシリアライズされ、
    /// CORS 設定を含むレスポンスヘッダーが自動的に付与されます。
    /// </summary>
    /// <typeparam name="T">レスポンスボディの型</typeparam>
    /// <param name="statusCode">HTTP ステータスコード</param>
    /// <param name="body">レスポンスボディ（API の結果データ）</param>
    /// <returns>
    /// CORS ヘッダー付きの <c>APIGatewayProxyResponse</c> オブジェクト
    /// </returns>
    private static APIGatewayProxyResponse BuildResponse<T>(int statusCode, ApiResponse<T> body) =>
        new()
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body),
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Access-Control-Allow-Origin"] = "*"   // 全てのオリジンを許可する（本番では指定）
            }
        };
    
    /// <summary>
    /// プリフライトリクエスト（CORS の OPTIONS リクエスト）に対する
    /// 200 OK の空レスポンスを生成します。
    /// 必要な CORS ヘッダーを含み、ボディは空です。
    /// </summary>
    /// <returns>
    /// CORS ヘッダー付きの <c>APIGatewayProxyResponse</c> オブジェクト（ステータスコード 200）
    /// </returns>
    public static APIGatewayProxyResponse Options() =>
        new()
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                ["Access-Control-Allow-Origin"] = "*",  // 全てのオリジンを許可する（本番では指定）
                ["Access-Control-Allow-Headers"] = "*", // クライアントが送ってくるリクエストヘッダーのうち、全てを許可
                ["Access-Control-Allow-Methods"] = "OPTIONS,POST,GET"  // どの HTTP メソッドを許可するかを指定
            }
        };
}

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string? Error { get; set; }
}
