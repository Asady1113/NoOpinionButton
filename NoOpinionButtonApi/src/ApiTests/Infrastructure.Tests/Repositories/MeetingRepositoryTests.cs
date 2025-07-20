using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Core.Domain.Entities;
using Infrastructure.Entities;
using Infrastructure.Repository;
using Moq;

namespace Infrastructure.Tests.Repositories;

public class MeetingRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _contextMock;
    private readonly MeetingRepository _repository;

    public MeetingRepositoryTests()
    {
        _contextMock = new Mock<IDynamoDBContext>();
        _repository = new MeetingRepository(_contextMock.Object);
    }

    [Fact]
    public async Task 正常系_GetMeetingByIdAsync_存在する会議IDで正常取得()
    {
        // Arrange
        var meetingId = "meeting123";
        var meetingEntity = new MeetingEntity
        {
            Id = meetingId,
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };

        _contextMock.Setup(x => x.LoadAsync<MeetingEntity>(meetingId, default))
            .ReturnsAsync(meetingEntity);

        // Act
        var result = await _repository.GetMeetingByIdAsync(meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(meetingId, result.Id);
        Assert.Equal("テスト会議", result.Name);
        Assert.Equal("admin123", result.FacilitatorPassword);
        Assert.Equal("user456", result.ParticipantPassword);
        
        _contextMock.Verify(x => x.LoadAsync<MeetingEntity>(meetingId, default), Times.Once);
    }

    [Fact]
    public async Task 異常系_GetMeetingByIdAsync_存在しない会議IDでKeyNotFoundException発生()
    {
        // Arrange
        var meetingId = "nonexistent-meeting";
        
        _contextMock.Setup(x => x.LoadAsync<MeetingEntity>(meetingId, default))
            .ReturnsAsync((MeetingEntity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _repository.GetMeetingByIdAsync(meetingId));
        
        Assert.Equal($"Meeting with Id '{meetingId}' was not found.", exception.Message);
        _contextMock.Verify(x => x.LoadAsync<MeetingEntity>(meetingId, default), Times.Once);
    }

    [Fact]
    public async Task 異常系_GetMeetingByIdAsync_DynamoDB接続エラー()
    {
        // Arrange
        var meetingId = "meeting123";
        var dynamoException = new AmazonDynamoDBException("DynamoDB connection error");

        _contextMock.Setup(x => x.LoadAsync<MeetingEntity>(meetingId, default))
            .ThrowsAsync(dynamoException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AmazonDynamoDBException>(() => 
            _repository.GetMeetingByIdAsync(meetingId));
        
        Assert.Equal("DynamoDB connection error", exception.Message);
        _contextMock.Verify(x => x.LoadAsync<MeetingEntity>(meetingId, default), Times.Once);
    }

    [Fact]
    public async Task 境界値_GetMeetingByIdAsync_特殊文字を含むID()
    {
        // Arrange
        var meetingId = "会議-123!@#$%";
        var meetingEntity = new MeetingEntity
        {
            Id = meetingId,
            Name = "特殊文字テスト会議",
            FacilitatorPassword = "パスワード123",
            ParticipantPassword = "ユーザー456"
        };

        _contextMock.Setup(x => x.LoadAsync<MeetingEntity>(meetingId, default))
            .ReturnsAsync(meetingEntity);

        // Act
        var result = await _repository.GetMeetingByIdAsync(meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(meetingId, result.Id);
        Assert.Equal("特殊文字テスト会議", result.Name);
        Assert.Equal("パスワード123", result.FacilitatorPassword);
        Assert.Equal("ユーザー456", result.ParticipantPassword);
        
        _contextMock.Verify(x => x.LoadAsync<MeetingEntity>(meetingId, default), Times.Once);
    }
}