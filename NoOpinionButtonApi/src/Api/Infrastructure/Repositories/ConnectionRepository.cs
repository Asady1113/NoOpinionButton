using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Connection;
using Core.Domain.ValueObjects.Meeting;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

/// <summary>
/// 接続リポジトリのDynamoDB実装
/// </summary>
public class ConnectionRepository : IConnectionRepository
{
    private readonly IDynamoDBContext _context;

    public ConnectionRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Connection> SaveAsync(Connection connection)
    {
        // ドメインエンティティをDynamoDBエンティティに変換
        var connectionEntity = new WebSocketConnectionEntity
        {
            ConnectionId = connection.Id,
            ParticipantId = connection.ParticipantId,
            MeetingId = connection.MeetingId,
            ConnectedAt = connection.ConnectedAt,
            IsActive = connection.IsActive
        };

        // DynamoDBに保存
        await _context.SaveAsync(connectionEntity);

        // 保存されたエンティティをドメインエンティティに変換して返す
        return new Connection(
            connectionEntity.ConnectionId,
            connectionEntity.ParticipantId,
            connectionEntity.MeetingId,
            connectionEntity.ConnectedAt,
            connectionEntity.IsActive
        );
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(MeetingId meetingId)
    {
        var queryConfig = new QueryOperationConfig
        {
            IndexName = "MeetingId-Index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "MeetingId = :meetingId",
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":meetingId", meetingId.Value }
                }
            },
            FilterExpression = new Expression
            {
                ExpressionStatement = "IsActive = :isActive",
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":isActive", true }
                }
            }
        };

        var search = _context.FromQueryAsync<WebSocketConnectionEntity>(queryConfig);
        var connectionEntities = await search.GetRemainingAsync();

        // DynamoDBエンティティをドメインエンティティに変換
        return connectionEntities.Select(entity => new Connection(
            entity.ConnectionId,
            entity.ParticipantId,
            entity.MeetingId,
            entity.ConnectedAt,
            entity.IsActive
        ));
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateAsync(ConnectionId connectionId)
    {
        try
        {
            var connectionEntity = await _context.LoadAsync<WebSocketConnectionEntity>(connectionId);
            
            if (connectionEntity == null)
                return false;

            connectionEntity.IsActive = false;
            await _context.SaveAsync(connectionEntity);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}