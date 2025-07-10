using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Core.Application;
using DependencyInjection;
using SignInFunction.DTOs;
using Common.ApiResponse;

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
    /// SignInエンドポイントのLambdaハンドラー
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            switch ((request.Path, request.HttpMethod))
            {
                case ("/signin", "POST"):
                    SignInResponse responseBody = await HandleSignIn(request);
                    return ApiResponseFactory.Ok(responseBody);
                default:
                    return ApiResponseFactory.NotFound("Requested endpoint was not found.");
            }
        }
        catch (ArgumentException ex)
        {
            return ApiResponseFactory.BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiResponseFactory.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogLine(ex.ToString());
            return ApiResponseFactory.ServerError();
        }
    }

    /// <summary>
    /// SignInエンドポイントの処理
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <returns>SignInResponse</returns>
    private async Task<SignInResponse> HandleSignIn(APIGatewayProxyRequest request)
    {
        SignInServiceRequest signInServiceRequest = CreateSignInServiceRequest(request.QueryStringParameters);
        SignInServiceResponse signInServiceResponse = await _signInService.SignInAsync(signInServiceRequest);

        return new SignInResponse
        {
            Id = signInServiceResponse.Id,
            MeetingId = signInServiceResponse.MeetingId,
            MeetingName = signInServiceResponse.MeetingName,
            IsFacilitator = signInServiceResponse.IsFacilitator,
        };
    }

    /// <summary>
    /// SignInServiceRequest を生成する。
    /// 必須: meetingId (int)、password (string)。
    /// </summary>
    /// <param name="queryParams">クエリパラメータ</param>
    /// <returns>SignInServiceRequest</returns>
    /// <exception cref="ArgumentNullException">queryParams が null の場合</exception>
    /// <exception cref="ArgumentException">パラメータが不足または不正な場合</exception>
    private SignInServiceRequest CreateSignInServiceRequest(IDictionary<string, string>? queryParams)
    {
        if (queryParams is null)
        {
            throw new ArgumentException("missing query parameter.");
        }

        if (!queryParams.TryGetValue("meetingId", out var meetingId) || string.IsNullOrEmpty(meetingId))
        {
            throw new ArgumentException("Invalid or missing 'meetingId' query parameter.");
        }

        if (!queryParams.TryGetValue("password", out var password) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Missing 'password' query parameter.");
        }

        return new SignInServiceRequest
        {
            MeetingId = meetingId,
            Password = password
        };
    }
}
