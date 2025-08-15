namespace Core.Domain.Entities;

/// <summary>
/// 参加者のリアルタイム接続状態を表すドメインエンティティ
/// </summary>
public class Connection
{
    /// <summary>
    /// 接続ID（主キー）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 参加者ID
    /// </summary>
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// 会議ID
    /// </summary>
    public string MeetingId { get; set; } = string.Empty;

    /// <summary>
    /// 接続開始時刻
    /// </summary>
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 接続状態
    /// </summary>
    public bool IsActive { get; set; } = true;
}