namespace Core.Application.DTOs.Requests;

public class ConnectServiceRequest
{
    /// <summary>
    /// WebSocket接続ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 会議ID（オプション - 接続時に指定される場合）
    /// </summary>
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 参加者ID（オプション - 接続時に指定される場合）
    /// </summary>
    public string ParticipantId { get; set; } = string.Empty;
}