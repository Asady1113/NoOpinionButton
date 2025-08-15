using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using DependencyInjection;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WebSocketDisconnectFunction;

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
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"WebSocket Disconnect - ConnectionId: {connectionId}");
            
            // WebSocket切断をサービス層で処理
            var disconnectRequest = new DisconnectServiceRequest
            {
                ConnectionId = connectionId
            };

            await _connectionService.DisconnectAsync(disconnectRequest);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected"
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error: {ex}");
            // 切断処理のエラーは成功として扱う（接続が既に存在しない場合など）
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected"
            };
        }
    }
}
