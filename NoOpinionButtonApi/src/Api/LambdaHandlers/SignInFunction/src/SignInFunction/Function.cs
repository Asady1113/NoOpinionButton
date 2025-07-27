using System.Text.Json;
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
                case ("/signin", "OPTIONS"):
                    return ApiResponseFactory.Options();
                default:
                    return ApiResponseFactory.NotFound("Requested endpoint was not found.");
            }
        }
        catch (ArgumentException ex)
        {
            return ApiResponseFactory.BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponseFactory.Unauthorized(ex.Message);
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
        SignInServiceRequest signInServiceRequest = CreateSignInServiceRequest(request.Body);
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
    /// 必須: meetingId, password (JSON)
    /// </summary>
    /// <param name="body">JSON Body</param>
    /// <returns>SignInServiceRequest</returns>
    /// <exception cref="ArgumentException">Body が null / 不正な場合</exception>
    private SignInServiceRequest CreateSignInServiceRequest(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentException("Missing request body.");
        }

        try
        {
            SignInRequest? request = JsonSerializer.Deserialize<SignInRequest>(body);
            if (request == null || string.IsNullOrEmpty(request.MeetingId) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Invalid request body. 'meetingId' and 'password' are required.");
            }

            SignInServiceRequest signInServiceRequest = new SignInServiceRequest
            {
                MeetingId = request.MeetingId,
                Password = request.Password,
            };
            return signInServiceRequest;
        }
        catch (JsonException)
        {
            throw new ArgumentException("Invalid JSON format in request body.");
        }
    }
}
