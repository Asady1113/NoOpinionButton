using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Application.Ports;
using Core.Domain.Entities;
using Core.Domain.Ports;

namespace Core.Application.Services;

public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository;

    public ConnectionService(IConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    /// <inheritdoc/>
    public async Task<ConnectServiceResponse> ConnectAsync(ConnectServiceRequest request)
    {
        // 接続エンティティ作成
        var connection = new Connection(
            request.ConnectionId,
            request.ParticipantId,
            request.MeetingId,
            DateTime.UtcNow,
            true
        );

        // 接続をDynamoDBに保存
        var savedConnection = await _connectionRepository.SaveAsync(connection);

        return new ConnectServiceResponse
        {
            ConnectionId = savedConnection.Id
        };
    }

    /// <inheritdoc/>
    public async Task<DisconnectServiceResponse> DisconnectAsync(DisconnectServiceRequest request)
    {
        // 接続を無効化
        await _connectionRepository.DeactivateAsync(request.ConnectionId);

        return new DisconnectServiceResponse
        {
            ConnectionId = request.ConnectionId
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId)
    {
        return await _connectionRepository.GetActiveConnectionsByMeetingIdAsync(meetingId);
    }
}