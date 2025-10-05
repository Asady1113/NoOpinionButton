using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;

namespace Core.Domain.Entities;

public class Participant
{
    /// <summary>
    /// 参加者ID（主キー）
    /// </summary>
    public ParticipantId Id { get; private set; }

    /// <summary>
    /// 参加者名
    /// </summary>
    public ParticipantName Name { get; private set; }

    /// <summary>
    /// 会議ID（外部キー）
    /// </summary>
    public MeetingId MeetingId { get; private set; }

    /// <summary>
    /// 意見ありませんボタンを押せる回数
    /// </summary>
    public NoOpinionPoint NoOpinionPoint { get; private set; }

    /// <summary>
    /// 意見を持っているか
    /// </summary>
    public bool HasOpinion { get; private set; }

    /// <summary>
    /// 会議に参加中かどうか
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// コンストラクタ（不変条件を強制）
    /// </summary>
    /// <param name="id">参加者ID</param>
    /// <param name="name">参加者名</param>
    /// <param name="meetingId">会議ID</param>
    /// <param name="noOpinionPoint">意見なしボタン押下可能回数</param>
    /// <param name="hasOpinion">意見保有状態</param>
    /// <param name="isActive">参加状態</param>
    public Participant(ParticipantId id, ParticipantName name, MeetingId meetingId, NoOpinionPoint noOpinionPoint, bool hasOpinion = true, bool isActive = true)
    {
        Id = id;
        Name = name;
        MeetingId = meetingId;
        NoOpinionPoint = noOpinionPoint;
        HasOpinion = hasOpinion;
        IsActive = isActive;
    }
}