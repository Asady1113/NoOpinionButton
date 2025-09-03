using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
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
}