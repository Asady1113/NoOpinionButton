using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

/// <summary>
/// WebSocket接続リポジトリのインターフェース (Infrastructure内部用)
/// Core層のPortsとは異なり、Infrastructure層内部の結合を疎にするためのもの
/// </summary>
public interface IWebSocketConnectionRepository
{
    /// <summary>
    /// WebSocket接続を保存する
    /// </summary>
    /// <param name="connectionEntity">保存する接続情報</param>
    /// <returns>保存された接続情報</returns>
    Task<WebSocketConnectionEntity> SaveAsync(WebSocketConnectionEntity connectionEntity);

    /// <summary>
    /// 会議IDで有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    Task<IEnumerable<WebSocketConnectionEntity>> GetActiveConnectionsByMeetingIdAsync(string meetingId);

    /// <summary>
    /// 接続を無効化する（切断時）
    /// </summary>
    /// <param name="connectionId">接続ID</param>
    /// <returns>更新成功可否</returns>
    Task<bool> DeactivateAsync(string connectionId);
}