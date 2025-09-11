namespace Core.Application;

/// <summary>
/// 参加者名前更新リクエスト
/// </summary>
public class ParticipantUpdateServiceRequest
{
    /// <summary>
    /// 更新対象の参加者ID
    /// </summary>
    public string ParticipantId { get; set; } = "";
    
    /// <summary>
    /// 新しい参加者名
    /// </summary>
    public string Name { get; set; } = "";
}