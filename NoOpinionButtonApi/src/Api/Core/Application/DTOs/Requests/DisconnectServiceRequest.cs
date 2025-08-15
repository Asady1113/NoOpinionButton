namespace Core.Application.DTOs.Requests;

public class DisconnectServiceRequest
{
    /// <summary>
    /// WebSocket接続ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;
}