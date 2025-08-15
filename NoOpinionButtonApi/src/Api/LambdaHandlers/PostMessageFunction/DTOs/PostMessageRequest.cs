using System.Text.Json.Serialization;

namespace PostMessageFunction.DTOs;

/// <summary>
/// メッセージ投稿リクエスト
/// </summary>
public class PostMessageRequest
{
    /// <summary>
    /// 会議ID
    /// </summary>
    [JsonPropertyName("meetingId")]
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 参加者ID
    /// </summary>
    [JsonPropertyName("participantId")]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}