namespace Core.Domain.Entities
{
    public class WebSocketConnection
    {
        /// <summary>
        /// WebSocket接続ID（主キー）
        /// API Gateway WebSocketが自動生成する一意のID
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 参加者ID（外部キー）
        /// </summary>
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// 会議ID（外部キー）
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// 接続開始時刻
        /// </summary>
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 接続状態
        /// true: 接続中, false: 切断済み
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}