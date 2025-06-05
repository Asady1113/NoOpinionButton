using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SignInFunction;

public class Function
{
    /// <summary>
    /// API Gateway からのリクエストを処理するLambdaハンドラー
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // TODO; ログインのエンドポイントから（フォルダ名等も変えたほうがいいかも）
        string path = request.Path;
        string method = request.HttpMethod;

        if (path == "/top" && method == "GET")
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "トップページのデータ"
            };
        }
        else
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = "Not Found"
            };
        }
    }
}
