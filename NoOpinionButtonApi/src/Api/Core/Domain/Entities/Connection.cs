using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Connection;
using Core.Domain.ValueObjects.Participant;

namespace Core.Domain.Entities;

/// <summary>
/// 参加者のリアルタイム接続状態を表すドメインエンティティ
/// </summary>
public class Connection
{
    /// <summary>
    /// 接続ID（主キー）
    /// </summary>
    public ConnectionId Id { get; private set; }

    /// <summary>
    /// 参加者ID
    /// </summary>
    public ParticipantId ParticipantId { get; private set; }

    /// <summary>
    /// 会議ID
    /// </summary>
    public MeetingId MeetingId { get; private set; }

    /// <summary>
    /// 接続開始時刻
    /// </summary>
    public DateTime ConnectedAt { get; private set; }

    /// <summary>
    /// 接続状態
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// コンストラクタ（不変条件を強制）
    /// </summary>
    /// <param name="id">接続ID</param>
    /// <param name="participantId">参加者ID</param>
    /// <param name="meetingId">会議ID</param>
    /// <param name="connectedAt">接続開始時刻</param>
    /// <param name="isActive">接続状態</param>
    public Connection(ConnectionId id, ParticipantId participantId, MeetingId meetingId, DateTime? connectedAt = null, bool isActive = true)
    {
        Id = id;
        ParticipantId = participantId;
        MeetingId = meetingId;
        ConnectedAt = connectedAt ?? DateTime.UtcNow;
        IsActive = isActive;
    }
}