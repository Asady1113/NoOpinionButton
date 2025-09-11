namespace Core.Application;

/// <summary>
/// 参加者名前更新レスポンス
/// </summary>
public class ParticipantUpdateServiceResponse
{
    /// <summary>
    /// 更新後の参加者名（成功時のみ）
    /// </summary>
    public string UpdatedName { get; set; } = "";
}