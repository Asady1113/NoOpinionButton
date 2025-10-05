using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;
using DependencyInjection;
using PostMessageFunction.DTOs;
using Common.ApiResponse;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PostMessageFunction;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageService _messageService;

    public Function()
    {
        // DI
        _serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        _messageService = _serviceProvider.GetRequiredService<IMessageService>();
    }

    /// <summary>
    /// PostMessageエンドポイントのLambdaハンドラー
    /// </summary>
    /// <param name="request">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            switch ((request.Path, request.HttpMethod))
            {
                case ("/message", "POST"):
                    PostMessageResponse responseBody = await HandlePostMessage(request);
                    return ApiResponseFactory.Ok(responseBody);
                case ("/message", "OPTIONS"):
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
    /// PostMessageエンドポイントの処理
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <returns>PostMessageResponse</returns>
    private async Task<PostMessageResponse> HandlePostMessage(APIGatewayProxyRequest request)
    {
        PostMessageRequest postMessageRequest = CreatePostMessageRequest(request.Body);
        var coreRequest = new PostMessageServiceRequest
        {
            MeetingId = postMessageRequest.MeetingId,
            ParticipantId = postMessageRequest.ParticipantId,
            Content = postMessageRequest.Content
        };

        var response = await _messageService.PostMessageAsync(coreRequest);

        return new PostMessageResponse
        {
            MessageId = response.MessageId
        };
    }

    /// <summary>
    /// PostMessageRequestを生成する。
    /// 必須: meetingId, participantId, content (JSON)
    /// </summary>
    /// <param name="body">JSON Body</param>
    /// <returns>PostMessageRequest</returns>
    /// <exception cref="ArgumentException">Body が null / 不正な場合</exception>
    private PostMessageRequest CreatePostMessageRequest(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentException("Missing request body.");
        }

        try
        {
            PostMessageRequest? request = JsonSerializer.Deserialize<PostMessageRequest>(body);
            if (request == null || 
                string.IsNullOrEmpty(request.MeetingId) || 
                string.IsNullOrEmpty(request.ParticipantId) || 
                string.IsNullOrEmpty(request.Content))
            {
                throw new ArgumentException("Invalid request body. 'meetingId', 'participantId', and 'content' are required.");
            }

            return request;
        }
        catch (JsonException)
        {
            throw new ArgumentException("Invalid JSON format in request body.");
        }
    }
}