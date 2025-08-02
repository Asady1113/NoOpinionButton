using System.Text.Json.Serialization;

namespace PostMessageFunction.DTOs;

/// <summary>
/// メッセージ投稿レスポンス
/// </summary>
public class PostMessageResponse
{
    /// <summary>
    /// 作成されたメッセージID
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}