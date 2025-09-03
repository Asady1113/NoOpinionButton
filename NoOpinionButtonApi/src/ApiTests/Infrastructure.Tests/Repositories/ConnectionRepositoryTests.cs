using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Core.Domain.Entities;
using Infrastructure.Entities;
using Infrastructure.Repository;
using Moq;

namespace Infrastructure.Tests.Repositories;

public class ConnectionRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _contextMock;
    private readonly ConnectionRepository _repository;

    public ConnectionRepositoryTests()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _repository = new ConnectionRepository(_contextMock.Object);
    }

    [Fact]
    public async Task 正常系_SaveAsync_接続エンティティの保存成功()
    {
        // Arrange
        var connectedAt = DateTime.UtcNow;
        var connection = new Connection(
            "conn-123",
            "participant-456",
            "meeting-789",
            connectedAt,
            true
        );

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveAsync(connection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(connection.Id, result.Id);
        Assert.Equal(connection.ParticipantId, result.ParticipantId);
        Assert.Equal(connection.MeetingId, result.MeetingId);
        Assert.Equal(connection.ConnectedAt, result.ConnectedAt);
        Assert.Equal(connection.IsActive, result.IsActive);

        _contextMock.Verify(x => x.SaveAsync(It.Is<WebSocketConnectionEntity>(entity =>
            entity.ConnectionId == connection.Id &&
            entity.ParticipantId == connection.ParticipantId &&
            entity.MeetingId == connection.MeetingId &&
            entity.ConnectedAt == connection.ConnectedAt &&
            entity.IsActive == connection.IsActive
        ), default), Times.Once);
    }

    [Fact]
    public async Task 正常系_GetActiveConnectionsByMeetingIdAsync_アクティブ接続の検索成功()
    {
        // Arrange
        var meetingId = "meeting-123";
        var connectedAt1 = DateTime.UtcNow.AddMinutes(-10);
        var connectedAt2 = DateTime.UtcNow.AddMinutes(-5);

        var connectionEntities = new List<WebSocketConnectionEntity>
        {
            new WebSocketConnectionEntity
            {
                ConnectionId = "conn-001",
                ParticipantId = "participant-001",
                MeetingId = meetingId,
                ConnectedAt = connectedAt1,
                IsActive = true
            },
            new WebSocketConnectionEntity
            {
                ConnectionId = "conn-002",
                ParticipantId = "participant-002",
                MeetingId = meetingId,
                ConnectedAt = connectedAt2,
                IsActive = true
            }
        };

        var mockSearch = new Mock<AsyncSearch<WebSocketConnectionEntity>>();
        mockSearch.Setup(x => x.GetRemainingAsync(default))
            .ReturnsAsync(connectionEntities);

        _contextMock.Setup(x => x.FromQueryAsync<WebSocketConnectionEntity>(It.IsAny<QueryOperationConfig>()))
            .Returns(mockSearch.Object);

        // Act
        var result = await _repository.GetActiveConnectionsByMeetingIdAsync(meetingId);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        
        Assert.Equal("conn-001", resultList[0].Id);
        Assert.Equal("participant-001", resultList[0].ParticipantId);
        Assert.Equal(meetingId, resultList[0].MeetingId);
        Assert.Equal(connectedAt1, resultList[0].ConnectedAt);
        Assert.True(resultList[0].IsActive);

        Assert.Equal("conn-002", resultList[1].Id);
        Assert.Equal("participant-002", resultList[1].ParticipantId);
        Assert.Equal(meetingId, resultList[1].MeetingId);
        Assert.Equal(connectedAt2, resultList[1].ConnectedAt);
        Assert.True(resultList[1].IsActive);

        _contextMock.Verify(x => x.FromQueryAsync<WebSocketConnectionEntity>(It.Is<QueryOperationConfig>(config =>
            config.IndexName == "MeetingId-Index"
        )), Times.Once);
    }

    [Fact]
    public async Task 正常系_DeactivateAsync_接続の非アクティブ化成功()
    {
        // Arrange
        var connectionId = "conn-deactivate";
        var connectionEntity = new WebSocketConnectionEntity
        {
            ConnectionId = connectionId,
            ParticipantId = "participant-123",
            MeetingId = "meeting-456",
            ConnectedAt = DateTime.UtcNow.AddMinutes(-15),
            IsActive = true
        };

        _contextMock.Setup(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default))
            .ReturnsAsync(connectionEntity);
        _contextMock.Setup(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.DeactivateAsync(connectionId);

        // Assert
        Assert.True(result);
        Assert.False(connectionEntity.IsActive);

        _contextMock.Verify(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default), Times.Once);
        _contextMock.Verify(x => x.SaveAsync(It.Is<WebSocketConnectionEntity>(entity =>
            entity.ConnectionId == connectionId &&
            entity.IsActive == false
        ), default), Times.Once);
    }

    [Fact]
    public async Task 異常系_DeactivateAsync_存在しない接続IDでfalseを返す()
    {
        // Arrange
        var connectionId = "nonexistent-connection";
        
        _contextMock.Setup(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default))
            .ReturnsAsync((WebSocketConnectionEntity)null);

        // Act
        var result = await _repository.DeactivateAsync(connectionId);

        // Assert
        Assert.False(result);
        _contextMock.Verify(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default), Times.Once);
        _contextMock.Verify(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default), Times.Never);
    }

    [Fact]
    public async Task 異常系_SaveAsync_DynamoDB例外時の適切な再スロー()
    {
        // Arrange
        var connection = new Connection(
            "conn-error",
            "participant-error",
            "meeting-error",
            DateTime.UtcNow,
            true
        );

        var dynamoException = new AmazonDynamoDBException("DynamoDB接続保存エラー");
        _contextMock.Setup(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() => 
            _repository.SaveAsync(connection));
        
        Assert.Equal("DynamoDB接続保存エラー", exception.Message);
        _contextMock.Verify(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task 異常系_DeactivateAsync_DynamoDB例外時にfalseを返す()
    {
        // Arrange
        var connectionId = "conn-exception";
        var dynamoException = new AmazonDynamoDBException("DynamoDB読み込みエラー");
        
        _contextMock.Setup(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default))
            .ThrowsAsync(dynamoException);

        // Act
        var result = await _repository.DeactivateAsync(connectionId);

        // Assert
        Assert.False(result);
        _contextMock.Verify(x => x.LoadAsync<WebSocketConnectionEntity>(connectionId, default), Times.Once);
        _contextMock.Verify(x => x.SaveAsync(It.IsAny<WebSocketConnectionEntity>(), default), Times.Never);
    }
}