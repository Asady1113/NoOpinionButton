using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
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

    /// <summary>
    /// 接続情報を保存する
    /// </summary>
    /// <param name="connection">保存する接続情報</param>
    /// <returns>保存された接続情報</returns>
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
        return new Connection
        {
            Id = connectionEntity.ConnectionId,
            ParticipantId = connectionEntity.ParticipantId,
            MeetingId = connectionEntity.MeetingId,
            ConnectedAt = connectionEntity.ConnectedAt,
            IsActive = connectionEntity.IsActive
        };
    }

    /// <summary>
    /// 会議IDで有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    public async Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId)
    {
        var queryConfig = new QueryOperationConfig
        {
            IndexName = "MeetingId-Index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "MeetingId = :meetingId AND IsActive = :isActive",
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":meetingId", meetingId },
                    { ":isActive", true }
                }
            }
        };

        var search = _context.FromQueryAsync<WebSocketConnectionEntity>(queryConfig);
        var connectionEntities = await search.GetRemainingAsync();

        // DynamoDBエンティティをドメインエンティティに変換
        return connectionEntities.Select(entity => new Connection
        {
            Id = entity.ConnectionId,
            ParticipantId = entity.ParticipantId,
            MeetingId = entity.MeetingId,
            ConnectedAt = entity.ConnectedAt,
            IsActive = entity.IsActive
        });
    }

    /// <summary>
    /// 接続を無効化する（切断時）
    /// </summary>
    /// <param name="connectionId">接続ID</param>
    /// <returns>更新成功可否</returns>
    public async Task<bool> DeactivateAsync(string connectionId)
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