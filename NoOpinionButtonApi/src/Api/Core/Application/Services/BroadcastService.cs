using Core.Application.Ports;
using Core.Domain.Ports;

namespace Core.Application.Services;

public class BroadcastService : IBroadcastService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IBroadcastRepository _broadcastRepository;

    public BroadcastService(IConnectionRepository connectionRepository, IBroadcastRepository broadcastRepository)
    {
        _connectionRepository = connectionRepository;
        _broadcastRepository = broadcastRepository;
    }

    /// <inheritdoc/>
    public async Task BroadcastMessageToMeetingAsync(string meetingId, string messageJson)
    {
        // 会議の有効な接続一覧を取得
        var activeConnections = await _connectionRepository.GetActiveConnectionsByMeetingIdAsync(meetingId);
        
        // 接続IDを抽出（暗黙的変換でstringに変換）
        var connectionIds = activeConnections.Select(connection => (string)connection.Id);
        
        // 一括配信を実行
        await _broadcastRepository.BroadcastToMultipleConnectionsAsync(connectionIds, messageJson);
    }
}