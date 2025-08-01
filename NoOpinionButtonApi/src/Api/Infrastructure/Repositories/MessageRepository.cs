using Amazon.DynamoDBv2.DataModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

/// <summary>
/// メッセージリポジトリのDynamoDB実装
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly IDynamoDBContext _context;

    public MessageRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// メッセージを保存する
    /// </summary>
    /// <param name="message">保存するメッセージ</param>
    /// <returns>保存されたメッセージ</returns>
    public async Task<Message> SaveAsync(Message message)
    {
        // ドメインエンティティをDynamoDBエンティティに変換
        var messageEntity = new MessageEntity
        {
            Id = message.Id,
            MeetingId = message.MeetingId,
            ParticipantId = message.ParticipantId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            LikeCount = message.LikeCount,
            ReportedCount = message.ReportedCount,
            IsActive = message.IsActive
        };

        // DynamoDBに保存
        await _context.SaveAsync(messageEntity);

        // 保存後のエンティティをドメインエンティティに変換して返却
        return new Message
        {
            Id = messageEntity.Id,
            MeetingId = messageEntity.MeetingId,
            ParticipantId = messageEntity.ParticipantId,
            Content = messageEntity.Content,
            CreatedAt = messageEntity.CreatedAt,
            LikeCount = messageEntity.LikeCount,
            ReportedCount = messageEntity.ReportedCount,
            IsActive = messageEntity.IsActive
        };
    }
}