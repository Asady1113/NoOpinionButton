using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Core.Domain.Entities;
using Core.Domain.ValueObjects.Participant;
using Infrastructure.Entities;
using Infrastructure.Repository;
using Moq;

namespace Infrastructure.Tests.Repositories;

public class ParticipantRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _contextMock;
    private readonly ParticipantRepository _repository;

    public ParticipantRepositoryTests()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _repository = new ParticipantRepository(_contextMock.Object);
    }

    [Fact]
    public async Task 正常系_SaveParticipantAsync_正常な参加者保存()
    {
        // Arrange
        var participantId = "participant123";
        var meetingId = "meeting456";

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveParticipantAsync(participantId, meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(participantId, result.Id);
        Assert.Equal(meetingId, result.MeetingId);
        
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p => 
            p.Id == participantId && p.MeetingId == meetingId), default), Times.Once);
    }

    [Fact]
    public async Task 異常系_SaveParticipantAsync_DynamoDB保存エラー()
    {
        // Arrange
        var participantId = "participant123";
        var meetingId = "meeting456";
        var dynamoException = new AmazonDynamoDBException("DynamoDB save error");

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() => 
            _repository.SaveParticipantAsync(participantId, meetingId));
        
        Assert.Equal("DynamoDB save error", exception.Message);
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p => 
            p.Id == participantId && p.MeetingId == meetingId), default), Times.Once);
    }

    [Fact]
    public async Task 境界値_SaveParticipantAsync_特殊文字を含むID()
    {
        // Arrange
        var participantId = "参加者-123!@#$%";
        var meetingId = "会議-456!@#$%";

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.SaveParticipantAsync(participantId, meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(participantId, result.Id);
        Assert.Equal(meetingId, result.MeetingId);
        
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p => 
            p.Id == participantId && p.MeetingId == meetingId), default), Times.Once);
    }

    // GetByIdAsync テストケース
    [Fact]
    public async Task 正常系_GetByIdAsync_存在する参加者を正常に取得()
    {
        // Arrange
        var participantId = new ParticipantId("participant123");
        var participantEntity = new ParticipantEntity
        {
            Id = participantId.Value,
            Name = "テスト参加者",
            MeetingId = "meeting123",
            NoOpinionPoint = 2,
            HasOpinion = true,
            IsActive = true
        };

        _contextMock.Setup(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default))
            .ReturnsAsync(participantEntity);

        // Act
        var result = await _repository.GetByIdAsync(participantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(participantId.Value, result.Id.Value);
        Assert.Equal("テスト参加者", result.Name.Value);
        Assert.Equal("meeting123", result.MeetingId.Value);
        Assert.Equal(2, result.NoOpinionPoint.Value);
        Assert.True(result.HasOpinion);
        Assert.True(result.IsActive);

        _contextMock.Verify(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default), Times.Once);
    }

    [Fact]
    public async Task 正常系_GetByIdAsync_存在しない参加者の場合null返却()
    {
        // Arrange
        var participantId = new ParticipantId("nonexistent123");

        _contextMock.Setup(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default))
            .ReturnsAsync((ParticipantEntity)null);

        // Act
        var result = await _repository.GetByIdAsync(participantId);

        // Assert
        Assert.Null(result);
        _contextMock.Verify(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default), Times.Once);
    }

    [Fact]
    public async Task 異常系_GetByIdAsync_DynamoDB取得エラー()
    {
        // Arrange
        var participantId = new ParticipantId("participant123");
        var dynamoException = new AmazonDynamoDBException("DynamoDB load error");

        _contextMock.Setup(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() =>
            _repository.GetByIdAsync(participantId));

        Assert.Equal("DynamoDB load error", exception.Message);
        _contextMock.Verify(x => x.LoadAsync<ParticipantEntity>(participantId.Value, default), Times.Once);
    }

    // UpdateAsync テストケース
    [Fact]
    public async Task 正常系_UpdateAsync_参加者情報を正常に更新()
    {
        // Arrange
        var participant = new Participant(
            "participant123",
            "更新された名前",
            "meeting123",
            2,
            true,
            true
        );

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateAsync(participant);

        // Assert
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p =>
            p.Id == participant.Id.Value &&
            p.Name == participant.Name.Value &&
            p.MeetingId == participant.MeetingId.Value &&
            p.NoOpinionPoint == participant.NoOpinionPoint.Value &&
            p.HasOpinion == participant.HasOpinion &&
            p.IsActive == participant.IsActive), default), Times.Once);
    }

    [Fact]
    public async Task 異常系_UpdateAsync_DynamoDB更新エラー()
    {
        // Arrange
        var participant = new Participant(
            "participant123",
            "更新された名前",
            "meeting123",
            1,
            false,
            false
        );
        var dynamoException = new AmazonDynamoDBException("DynamoDB update error");

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() =>
            _repository.UpdateAsync(participant));

        Assert.Equal("DynamoDB update error", exception.Message);
        _contextMock.Verify(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task 境界値_UpdateAsync_NoOpinionPoint最大値での更新()
    {
        // Arrange
        var participant = new Participant(
            "participant123",
            "境界値テスト",
            "meeting123",
            2, // 最大値
            true,
            true
        );

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateAsync(participant);

        // Assert
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p =>
            p.NoOpinionPoint == 2), default), Times.Once);
    }

    [Fact]
    public async Task 境界値_UpdateAsync_非アクティブ参加者の更新()
    {
        // Arrange
        var participant = new Participant(
            "participant123",
            "非アクティブ参加者",
            "meeting123",
            0,
            false,
            false // IsActive = false
        );

        _contextMock.Setup(x => x.SaveAsync(It.IsAny<ParticipantEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateAsync(participant);

        // Assert
        _contextMock.Verify(x => x.SaveAsync(It.Is<ParticipantEntity>(p =>
            p.IsActive == false), default), Times.Once);
    }
}