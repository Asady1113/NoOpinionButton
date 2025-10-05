using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Core.Application.Ports;
using DependencyInjection;
using UpdateParticipantNameFunction.DTOs;
using Common.ApiResponse;
using Core.Application;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UpdateParticipantNameFunction;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IParticipantUpdateService _participantUpdateService;

    public Function()
    {
        // DI
        _serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        _participantUpdateService = _serviceProvider.GetRequiredService<IParticipantUpdateService>();
    }

    /// <summary>
    /// UpdateParticipantNameエンドポイントのLambdaハンドラー
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
                case (var path, "PUT") when path?.Contains("/participants/") == true && path.EndsWith("/name"):
                    UpdateParticipantNameResponse responseBody = await HandleUpdateParticipantName(request);
                    return ApiResponseFactory.Ok(responseBody);
                case (_, "OPTIONS"):
                    return ApiResponseFactory.Options();
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
        catch (InvalidOperationException ex)
        {
            return ApiResponseFactory.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogLine(ex.ToString());
            return ApiResponseFactory.ServerError();
        }
    }

    /// <summary>
    /// UpdateParticipantNameエンドポイントの処理
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <returns>UpdateParticipantNameResponse</returns>
    private async Task<UpdateParticipantNameResponse> HandleUpdateParticipantName(APIGatewayProxyRequest request)
    {
        string participantId = ExtractParticipantId(request);
        ParticipantUpdateServiceRequest participantUpdateServiceRequest = CreateUpdateRequest(request.Body, participantId);
        
        var serviceResponse = await _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest);

        return new UpdateParticipantNameResponse
        {
            UpdatedName = serviceResponse.UpdatedName
        };
    }

    /// <summary>
    /// パスパラメータから参加者IDを抽出する
    /// </summary>
    /// <param name="request">APIGatewayProxyRequest</param>
    /// <returns>参加者ID</returns>
    /// <exception cref="ArgumentException">参加者IDが見つからない場合</exception>
    private string ExtractParticipantId(APIGatewayProxyRequest request)
    {
        if (request.PathParameters?.TryGetValue("participantId", out string? participantId) == true && 
            !string.IsNullOrEmpty(participantId))
        {
            return participantId;
        }
        
        throw new ArgumentException("Missing participantId in path parameters.");
    }

    /// <summary>
    /// ParticipantUpdateServiceRequest を生成する。
    /// 必須: name (JSON)
    /// </summary>
    /// <param name="body">JSON Body</param>
    /// <param name="participantId">参加者ID</param>
    /// <returns>ParticipantUpdateServiceRequest</returns>
    /// <exception cref="ArgumentException">Body が null / 不正な場合</exception>
    private ParticipantUpdateServiceRequest CreateUpdateRequest(string? body, string participantId)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentException("Missing request body.");
        }

        try
        {
            UpdateParticipantNameRequest? request = JsonSerializer.Deserialize<UpdateParticipantNameRequest>(body);
            if (request == null || string.IsNullOrEmpty(request.Name) )
            {
                throw new ArgumentException("Invalid request body. 'name' is required.");
            }

            ParticipantUpdateServiceRequest participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
            {
                ParticipantId = participantId,
                Name = request.Name
            };

            return participantUpdateServiceRequest;
        }
        catch (JsonException)
        {
            throw new ArgumentException("Invalid JSON format in request body.");
        }
    }
}