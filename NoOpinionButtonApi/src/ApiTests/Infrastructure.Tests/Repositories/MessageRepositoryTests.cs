using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Core.Domain.Entities;
using Infrastructure.Entities;
using Infrastructure.Repository;
using Moq;

namespace Infrastructure.Tests.Repositories;

public class MessageRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _contextMock;
    private readonly MessageRepository _repository;

    public MessageRepositoryTests()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _repository = new MessageRepository(_contextMock.Object);
    }

    [Fact]
    public async Task 正常系_SaveAsync_メッセージエンティティの保存成功()
    {
        // Arrange
        var message = new Message(
            "msg-123",
            "meeting-456",
            "participant-789",
            "テストメッセージ",
            DateTime.UtcNow,
            0,
            0,
            true
        );

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<MessageEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(message.Id, result.Id);
        Assert.Equal(message.MeetingId, result.MeetingId);
        Assert.Equal(message.ParticipantId, result.ParticipantId);
        Assert.Equal(message.Content, result.Content);
        Assert.Equal(message.CreatedAt, result.CreatedAt);
        Assert.Equal(message.LikeCount, result.LikeCount);
        Assert.Equal(message.ReportedCount, result.ReportedCount);
        Assert.Equal(message.IsActive, result.IsActive);

        _contextMock.Verify(x => x.SaveAsync(It.Is<MessageEntity>(entity =>
            entity.Id == message.Id &&
            entity.MeetingId == message.MeetingId &&
            entity.ParticipantId == message.ParticipantId &&
            entity.Content == message.Content &&
            entity.CreatedAt == message.CreatedAt &&
            entity.LikeCount == message.LikeCount &&
            entity.ReportedCount == message.ReportedCount &&
            entity.IsActive == message.IsActive
        ), default), Times.Once);
    }

    [Fact]
    public async Task 正常系_SaveAsync_ドメインエンティティからDynamoDBエンティティへの変換確認()
    {
        // Arrange
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var message = new Message(
            "message-id-001",
            "meeting-id-002",
            "participant-id-003",
            "変換テストメッセージ",
            createdAt,
            5,
            2,
            false
        );

        MessageEntity capturedEntity = null;
        _contextMock.Setup(x => x.SaveAsync(It.IsAny<MessageEntity>(), default))
            .Callback<MessageEntity, CancellationToken>((entity, token) => capturedEntity = entity)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveAsync(message);

        // Assert - DynamoDBエンティティへの正確な変換を確認
        Assert.NotNull(capturedEntity);
        Assert.Equal("message-id-001", capturedEntity.Id);
        Assert.Equal("meeting-id-002", capturedEntity.MeetingId);
        Assert.Equal("participant-id-003", capturedEntity.ParticipantId);
        Assert.Equal("変換テストメッセージ", capturedEntity.Content);
        Assert.Equal(createdAt, capturedEntity.CreatedAt);
        Assert.Equal(5, capturedEntity.LikeCount);
        Assert.Equal(2, capturedEntity.ReportedCount);
        Assert.False(capturedEntity.IsActive);

        // 戻り値もドメインエンティティに正しく変換されていることを確認
        Assert.Equal(message.Id, result.Id);
        Assert.Equal(message.MeetingId, result.MeetingId);
        Assert.Equal(message.ParticipantId, result.ParticipantId);
        Assert.Equal(message.Content, result.Content);
        Assert.Equal(message.CreatedAt, result.CreatedAt);
        Assert.Equal(message.LikeCount, result.LikeCount);
        Assert.Equal(message.ReportedCount, result.ReportedCount);
        Assert.Equal(message.IsActive, result.IsActive);
    }

    [Fact]
    public async Task 異常系_SaveAsync_DynamoDB例外時の適切な再スロー()
    {
        // Arrange
        var message = new Message(
            "msg-error",
            "meeting-error",
            "participant-error",
            "エラーテストメッセージ",
            DateTime.UtcNow,
            1,
            0,
            true
        );

        var dynamoException = new AmazonDynamoDBException("DynamoDB保存エラー");
        _contextMock.Setup(x => x.SaveAsync(It.IsAny<MessageEntity>(), default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() => 
            _repository.SaveAsync(message));
        
        Assert.Equal("DynamoDB保存エラー", exception.Message);
        _contextMock.Verify(x => x.SaveAsync(It.IsAny<MessageEntity>(), default), Times.Once);
    }
}