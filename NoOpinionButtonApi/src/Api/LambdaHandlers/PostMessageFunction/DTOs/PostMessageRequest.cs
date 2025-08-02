namespace PostMessageFunction.DTOs;

/// <summary>
/// メッセージ投稿リクエスト
/// </summary>
public class PostMessageRequest
{
    /// <summary>
    /// 会議ID
    /// </summary>
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 参加者ID
    /// </summary>
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
}