namespace Core.Application.DTOs.Responses;

public class DisconnectServiceResponse
{
    /// <summary>
    /// 切断されたConnectionID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;
}