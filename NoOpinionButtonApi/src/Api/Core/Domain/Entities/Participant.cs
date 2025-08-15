namespace Core.Domain.Entities;

public class Participant
{
    /// <summary>
    /// 参加者ID（主キー）
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// 参加者名
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 会議ID（外部キー）
    /// </summary>
    public string MeetingId { get; set; } = "";

    /// <summary>
    /// 意見ありませんボタンを押せる回数
    /// </summary>
    public int NoOpinionPoint { get; set; } = 0;

    /// <summary>
    /// 意見を持っているか
    /// </summary>
    public bool HasOpinion { get; set; } = true;

    /// <summary>
    /// 会議に参加中かどうか
    /// </summary>
    public bool IsActive { get; set; } = true;
}