using Amazon.DynamoDBv2.DataModel;

namespace Infrastructure.Entities;

/// <summary>
/// メッセージのDynamoDBエンティティ
/// </summary>
[DynamoDBTable("Message")]
public class MessageEntity
{
    /// <summary>
    /// メッセージID（主キー）
    /// </summary>
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 会議ID（外部キー）
    /// </summary>
    [DynamoDBProperty]
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 作成者の参加者ID（外部キー）
    /// </summary>
    [DynamoDBProperty]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// メッセージ内容
    /// </summary>
    [DynamoDBProperty]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 「いいね」の数
    /// </summary>
    [DynamoDBProperty]
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// 通報された回数
    /// </summary>
    [DynamoDBProperty]
    public int ReportedCount { get; set; } = 0;

    /// <summary>
    /// メッセージが有効かどうか
    /// </summary>
    [DynamoDBProperty]
    public bool IsActive { get; set; } = true;
}