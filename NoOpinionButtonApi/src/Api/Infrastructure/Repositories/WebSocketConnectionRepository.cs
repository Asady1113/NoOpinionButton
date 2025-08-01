using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.Repository;

/// <summary>
/// WebSocket接続リポジトリのDynamoDB実装
/// </summary>
public class WebSocketConnectionRepository : IWebSocketConnectionRepository
{
    private readonly IDynamoDBContext _context;

    public WebSocketConnectionRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// WebSocket接続を保存する
    /// </summary>
    /// <param name="connectionEntity">保存する接続情報</param>
    /// <returns>保存された接続情報</returns>
    public async Task<WebSocketConnectionEntity> SaveAsync(WebSocketConnectionEntity connectionEntity)
    {
        // DynamoDBに保存
        await _context.SaveAsync(connectionEntity);
        return connectionEntity;
    }

    /// <summary>
    /// 会議IDで有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    public async Task<IEnumerable<WebSocketConnectionEntity>> GetActiveConnectionsByMeetingIdAsync(string meetingId)
    {
        // MeetingIdによるクエリ条件を設定
        var queryConfig = new QueryOperationConfig
        {
            // DynamoDBのセカンダリインデックスの識別名
            IndexName = "MeetingId-Index",
            KeyExpression = new Expression
            {
                // MeetingIdとIsActiveの両方が一致する条件 を指定
                ExpressionStatement = "MeetingId = :meetingId AND IsActive = :isActive",
                // プレースホルダーの実際の値を設定
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":meetingId", meetingId },
                    { ":isActive", true }
                }
            }
        };

        // クエリ実行
        // DynamoDBのテーブルに対して非同期でクエリを実行する
        var search = _context.FromQueryAsync<WebSocketConnectionEntity>(queryConfig);
        // クエリ結果をすべて取得
        return await search.GetRemainingAsync();
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
            // 既存の接続を取得
            var connectionEntity = await _context.LoadAsync<WebSocketConnectionEntity>(connectionId);
            
            if (connectionEntity == null)
                return false;

            // IsActiveをfalseに更新
            connectionEntity.IsActive = false;
            
            // 更新を保存
            await _context.SaveAsync(connectionEntity);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}