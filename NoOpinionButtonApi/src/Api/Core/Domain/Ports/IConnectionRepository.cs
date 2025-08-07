using Core.Domain.Entities;

namespace Core.Domain.Ports;

/// <summary>
/// 参加者のリアルタイム接続管理リポジトリ
/// </summary>
public interface IConnectionRepository
{
    /// <summary>
    /// 接続情報を保存する
    /// </summary>
    /// <param name="connection">保存する接続情報</param>
    /// <returns>保存された接続情報</returns>
    Task<Connection> SaveAsync(Connection connection);

    /// <summary>
    /// 会議IDで有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId);

    /// <summary>
    /// 接続を無効化する（切断時）
    /// </summary>
    /// <param name="connectionId">接続ID</param>
    /// <returns>更新成功可否</returns>
    Task<bool> DeactivateAsync(string connectionId);
}