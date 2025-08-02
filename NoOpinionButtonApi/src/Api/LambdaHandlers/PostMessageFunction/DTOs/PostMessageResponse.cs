namespace PostMessageFunction.DTOs;

/// <summary>
/// メッセージ投稿レスポンス
/// </summary>
public class PostMessageResponse
{
    /// <summary>
    /// 作成されたメッセージID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
}