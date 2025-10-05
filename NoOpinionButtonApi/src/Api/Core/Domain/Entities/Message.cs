using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Message;
using Core.Domain.ValueObjects.Participant;

namespace Core.Domain.Entities
{
    public class Message
    {
        /// <summary>
        /// メッセージID（主キー）
        /// </summary>
        public MessageId Id { get; private set; }

        /// <summary>
        /// 会議ID（外部キー）
        /// </summary>
        public MeetingId MeetingId { get; private set; }

        /// <summary>
        /// 作成者の参加者ID（外部キー）
        /// </summary>
        public ParticipantId ParticipantId { get; private set; }

        /// <summary>
        /// 参加者名
        /// </summary>
        public ParticipantName ParticipantName { get; private set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// メッセージ内容
        /// </summary>
        public MessageContent Content { get; private set; }

        /// <summary>
        /// 「いいね」の数
        /// </summary>
        public LikeCount LikeCount { get; private set; } = new LikeCount();

        /// <summary>
        /// 通報された回数
        /// </summary>
        public ReportedCount ReportedCount { get; private set; } = new ReportedCount();

        /// <summary>
        /// メッセージが有効かどうか
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// コンストラクタ（不変条件を強制）
        /// </summary>
        /// <param name="id">メッセージID</param>
        /// <param name="meetingId">会議ID</param>
        /// <param name="participantId">参加者ID</param>
        /// <param name="participantName">参加者名</param>
        /// <param name="content">メッセージ内容</param>
        /// <param name="createdAt">作成日時</param>
        /// <param name="likeCount">いいね数</param>
        /// <param name="reportedCount">通報回数</param>
        /// <param name="isActive">有効状態</param>
        public Message(MessageId id, MeetingId meetingId, ParticipantId participantId, ParticipantName participantName, MessageContent content, 
                      DateTime? createdAt = null, LikeCount likeCount = default, ReportedCount reportedCount = default, bool isActive = true)
        {
            Id = id;
            MeetingId = meetingId;
            ParticipantId = participantId;
            ParticipantName = participantName;
            Content = content;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            LikeCount = likeCount;
            ReportedCount = reportedCount;
            IsActive = isActive;
        }
    }
}