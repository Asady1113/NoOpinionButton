using Amazon.DynamoDBv2.DataModel;

namespace Infrastructure.Entities;

/// <summary>
/// WebSocket接続のDynamoDBエンティティ
/// </summary>
[DynamoDBTable("WebSocketConnection")]
public class WebSocketConnectionEntity
{
    /// <summary>
    /// WebSocket接続ID（主キー）
    /// API Gateway WebSocketが自動生成する一意のID
    /// </summary>
    [DynamoDBHashKey]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 参加者ID（外部キー）
    /// </summary>
    [DynamoDBProperty]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// 会議ID（外部キー）
    /// GSIのパーティションキーとして使用
    /// </summary>
    [DynamoDBProperty]
    [DynamoDBGlobalSecondaryIndexHashKey("MeetingId-Index")]
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 接続開始時刻
    /// </summary>
    [DynamoDBProperty]
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 接続状態
    /// true: 接続中, false: 切断済み
    /// </summary>
    [DynamoDBProperty]
    public bool IsActive { get; set; } = true;
}