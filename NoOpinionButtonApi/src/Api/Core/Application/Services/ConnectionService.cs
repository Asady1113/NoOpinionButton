using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Application.Ports;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;

namespace Core.Application.Services;

public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository;

    public ConnectionService(IConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    /// <summary>
    /// WebSocket接続を処理する
    /// </summary>
    /// <param name="request">接続リクエスト</param>
    /// <returns>接続レスポンス</returns>
    public async Task<ConnectServiceResponse> ConnectAsync(ConnectServiceRequest request)
    {
        // 接続エンティティ作成
        var connection = new Connection
        {
            Id = request.ConnectionId,
            MeetingId = request.MeetingId,
            ParticipantId = request.ParticipantId,
            ConnectedAt = DateTime.UtcNow,
            IsActive = true
        };

        // 接続をDynamoDBに保存
        var savedConnection = await _connectionRepository.SaveAsync(connection);

        return new ConnectServiceResponse
        {
            ConnectionId = savedConnection.Id
        };
    }

    /// <summary>
    /// WebSocket切断を処理する
    /// </summary>
    /// <param name="request">切断リクエスト</param>
    /// <returns>切断レスポンス</returns>
    public async Task<DisconnectServiceResponse> DisconnectAsync(DisconnectServiceRequest request)
    {
        // 接続を無効化
        await _connectionRepository.DeactivateAsync(request.ConnectionId);

        return new DisconnectServiceResponse
        {
            ConnectionId = request.ConnectionId
        };
    }

    /// <summary>
    /// 会議の有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    public async Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId)
    {
        return await _connectionRepository.GetActiveConnectionsByMeetingIdAsync(meetingId);
    }
}