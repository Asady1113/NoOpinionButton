using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Core.Application;
using DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SignInFunction;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignInService _signInService;

    public Function()
    {
        // DI
        _serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        _signInService = _serviceProvider.GetRequiredService<ISignInService>();
    }

    /// <summary>
    /// API Gateway からのリクエストを処理するLambdaハンドラー
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        string path = request.Path;
        string method = request.HttpMethod;
        IDictionary<string, string>? queryParams = request.QueryStringParameters;

        if (path == "/participant" && method == "POST")
        {
            // TODO; リファクタリング
            var signInServiceRequest = new SignInServiceRequest
            {
                MeetingId = int.Parse(queryParams["meetingId"]),
                Password = queryParams["password"]
            };
            SignInServiceResponse signInServiceResponse = await _signInService.SignInAsync(signInServiceRequest);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = $"Id:{signInServiceResponse.Id},MeetingId:{signInServiceResponse.MeetingId}"
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
