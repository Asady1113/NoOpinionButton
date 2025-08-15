using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using DependencyInjection;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WebSocketConnectFunction;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionService _connectionService;

    public Function()
    {
        _serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        _connectionService = _serviceProvider.GetRequiredService<IConnectionService>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // WebSocket API が発行するWebSocketの接続IDを取得
            // クライアントが接続したらこのLambdaが呼ばれることになってる
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"WebSocket Connect - ConnectionId: {connectionId}");
            
            // クエリパラメータから会議ID・参加者IDを取得（後で実装）
            var meetingId = GetQueryParameter(request, "meetingId") ?? "";
            var participantId = GetQueryParameter(request, "participantId") ?? "";
            
            // WebSocket接続をサービス層で処理
            var connectRequest = new ConnectServiceRequest
            {
                ConnectionId = connectionId,
                MeetingId = meetingId,
                ParticipantId = participantId
            };

            await _connectionService.ConnectAsync(connectRequest);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Connected"
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error: {ex}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = "Internal Server Error"
            };
        }
    }

    /// <summary>
    /// クエリパラメータから値を取得する
    /// </summary>
    /// <param name="request">APIGatewayProxyRequest</param>
    /// <param name="key">パラメータキー</param>
    /// <returns>パラメータ値（存在しない場合はnull）</returns>
    private string? GetQueryParameter(APIGatewayProxyRequest request, string key)
    {
        if (request.QueryStringParameters?.ContainsKey(key) == true)
        {
            return request.QueryStringParameters[key];
        }
        return null;
    }
}
