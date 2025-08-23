using Core.Application;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;
using Moq;

namespace Core.Tests.Application.Services;

public class SignInServiceTests
{
    private readonly Mock<IParticipantRepository> _participantRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly SignInService _signInService;

    public SignInServiceTests()
    {
        _participantRepositoryMock = new Mock<IParticipantRepository>();
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _signInService = new SignInService(_participantRepositoryMock.Object, _meetingRepositoryMock.Object);
    }

    [Fact]
    public async Task 正常系_SignInAsync_司会者パスワードで正常サインイン()
    {
        // Arrange
        var request = new SignInServiceRequest
        {
            MeetingId = "meeting123",
            Password = "facilitator-password"
        };

        var meeting = new Meeting(
            "meeting123",
            "テスト会議",
            "facilitator-password",
            "participant-password"
        );

        _meetingRepositoryMock.Setup(x => x.GetMeetingByIdAsync("meeting123"))
            .ReturnsAsync(meeting);

        // Act
        var result = await _signInService.SignInAsync(request);

        // Assert
        Assert.Equal("meeting123", result.MeetingId);
        Assert.Equal("テスト会議", result.MeetingName);
        Assert.True(result.IsFacilitator);
        Assert.Equal("", result.Id);
        
        _meetingRepositoryMock.Verify(x => x.GetMeetingByIdAsync("meeting123"), Times.Once);
        _participantRepositoryMock.Verify(x => x.SaveParticipantAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task 正常系_SignInAsync_参加者パスワードで正常サインイン()
    {
        // Arrange
        var request = new SignInServiceRequest
        {
            MeetingId = "meeting123",
            Password = "participant-password"
        };

        var meeting = new Meeting(
            "meeting123",
            "テスト会議",
            "facilitator-password",
            "participant-password"
        );

        var participant = new Participant(
            "participant123",
            "未設定",
            "meeting123",
            0
        );

        _meetingRepositoryMock.Setup(x => x.GetMeetingByIdAsync("meeting123"))
            .ReturnsAsync(meeting);
        _participantRepositoryMock.Setup(x => x.SaveParticipantAsync(It.IsAny<string>(), "meeting123"))
            .ReturnsAsync(participant);

        // Act
        var result = await _signInService.SignInAsync(request);

        // Assert
        Assert.Equal("participant123", result.Id);
        Assert.Equal("meeting123", result.MeetingId);
        Assert.Equal("テスト会議", result.MeetingName);
        Assert.False(result.IsFacilitator);
        
        _meetingRepositoryMock.Verify(x => x.GetMeetingByIdAsync("meeting123"), Times.Once);
        _participantRepositoryMock.Verify(x => x.SaveParticipantAsync(It.IsAny<string>(), "meeting123"), Times.Once);
    }

    [Fact]
    public async Task 異常系_SignInAsync_無効なパスワードでサインイン()
    {
        // Arrange
        var request = new SignInServiceRequest
        {
            MeetingId = "meeting123",
            Password = "invalid-password"
        };

        var meeting = new Meeting(
            "meeting123",
            "テスト会議",
            "facilitator-password",
            "participant-password"
        );

        _meetingRepositoryMock.Setup(x => x.GetMeetingByIdAsync("meeting123"))
            .ReturnsAsync(meeting);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _signInService.SignInAsync(request));
        
        Assert.Equal("Password is invalid", exception.Message);
        _meetingRepositoryMock.Verify(x => x.GetMeetingByIdAsync("meeting123"), Times.Once);
        _participantRepositoryMock.Verify(x => x.SaveParticipantAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_SignInAsync_存在しない会議IDでサインイン()
    {
        // Arrange
        var request = new SignInServiceRequest
        {
            MeetingId = "nonexistent-meeting",
            Password = "any-password"
        };

        _meetingRepositoryMock.Setup(x => x.GetMeetingByIdAsync("nonexistent-meeting"))
            .ThrowsAsync(new KeyNotFoundException($"Meeting with Id '{request.MeetingId}' was not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _signInService.SignInAsync(request));
        
        Assert.Equal($"Meeting with Id '{request.MeetingId}' was not found.", exception.Message);
        _meetingRepositoryMock.Verify(x => x.GetMeetingByIdAsync("nonexistent-meeting"), Times.Once);
        _participantRepositoryMock.Verify(x => x.SaveParticipantAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_SignInAsync_参加者作成でリポジトリエラー()
    {
        // Arrange
        var request = new SignInServiceRequest
        {
            MeetingId = "meeting123",
            Password = "participant-password"
        };

        var meeting = new Meeting(
            "meeting123",
            "テスト会議",
            "facilitator-password",
            "participant-password"
        );

        _meetingRepositoryMock.Setup(x => x.GetMeetingByIdAsync("meeting123"))
            .ReturnsAsync(meeting);
        _participantRepositoryMock.Setup(x => x.SaveParticipantAsync(It.IsAny<string>(), "meeting123"))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _signInService.SignInAsync(request));
        
        Assert.Equal("Database error", exception.Message);
        _meetingRepositoryMock.Verify(x => x.GetMeetingByIdAsync("meeting123"), Times.Once);
        _participantRepositoryMock.Verify(x => x.SaveParticipantAsync(It.IsAny<string>(), "meeting123"), Times.Once);
    }
}