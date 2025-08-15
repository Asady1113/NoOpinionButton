namespace Core.Domain.Ports;

/// <summary>
/// メッセージ配信リポジトリ
/// </summary>
public interface IBroadcastRepository
{
    /// <summary>
    /// 接続ID宛にメッセージを配信する
    /// </summary>
    /// <param name="connectionId">接続ID</param>
    /// <param name="message">配信するメッセージ</param>
    /// <returns>配信成功可否</returns>
    Task<bool> BroadcastToConnectionAsync(string connectionId, string message);

    /// <summary>
    /// 複数の接続ID宛にメッセージを一括配信する
    /// </summary>
    /// <param name="connectionIds">接続ID一覧</param>
    /// <param name="message">配信するメッセージ</param>
    /// <returns>配信結果（成功した接続数）</returns>
    Task<int> BroadcastToMultipleConnectionsAsync(IEnumerable<string> connectionIds, string message);
}