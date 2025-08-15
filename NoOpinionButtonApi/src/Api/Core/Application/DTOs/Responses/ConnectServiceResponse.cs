namespace Core.Application.DTOs.Responses;

public class ConnectServiceResponse
{
    /// <summary>
    /// 接続が成功したConnectionID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;
}