namespace Core.Domain.Entities
{
    public class Message
    {
        /// <summary>
        /// メッセージID（主キー）
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 会議ID（外部キー）
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// 作成者の参加者ID（外部キー）
        /// </summary>
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// メッセージ内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 「いいね」の数
        /// </summary>
        public int LikeCount { get; set; } = 0;

        /// <summary>
        /// 通報された回数
        /// </summary>
        public int ReportedCount { get; set; } = 0;

        /// <summary>
        /// メッセージが有効かどうか
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}