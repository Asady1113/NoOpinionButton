using Core.Application.DTOs.Requests;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Connection;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;
using Moq;

namespace Core.Tests.Application.Services;

public class ConnectionServiceTests
{
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock;
    private readonly ConnectionService _connectionService;

    public ConnectionServiceTests()
    {
        _connectionRepositoryMock = new Mock<IConnectionRepository>();
        _connectionService = new ConnectionService(_connectionRepositoryMock.Object);
    }

    [Fact]
    public async Task 正常系_ConnectAsync_Repositoryに正しいConnectionエンティティが保存される()
    {
        // Arrange
        var request = new ConnectServiceRequest
        {
            ConnectionId = "connection123",
            ParticipantId = "participant123",
            MeetingId = "meeting123"
        };

        var savedConnection = new Connection(
            new ConnectionId("connection123"),
            new ParticipantId("participant123"),
            new MeetingId("meeting123"),
            DateTime.UtcNow,
            true
        );

        _connectionRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Connection>()))
            .ReturnsAsync(savedConnection);

        // Act
        var result = await _connectionService.ConnectAsync(request);

        // Assert
        Assert.Equal("connection123", result.ConnectionId);

        // Repository.SaveAsyncが正しいプロパティを持つConnectionエンティティで呼ばれたことを検証
        _connectionRepositoryMock.Verify(x => x.SaveAsync(It.Is<Connection>(c =>
            c.Id.Value == "connection123" &&
            c.ParticipantId.Value == "participant123" &&
            c.MeetingId.Value == "meeting123" &&
            c.IsActive
        )), Times.Once);
    }

    [Fact]
    public async Task 正常系_DisconnectAsync_指定した接続IDで非アクティブ化される()
    {
        // Arrange
        var request = new DisconnectServiceRequest
        {
            ConnectionId = "connection456"
        };

        _connectionRepositoryMock.Setup(x => x.DeactivateAsync(It.IsAny<ConnectionId>()))
            .ReturnsAsync(true);

        // Act
        var result = await _connectionService.DisconnectAsync(request);

        // Assert
        Assert.Equal("connection456", result.ConnectionId);

        // Repository.DeactivateAsyncが正しい接続IDで呼ばれたことを検証
        _connectionRepositoryMock.Verify(x => x.DeactivateAsync(
            "connection456"
        ), Times.Once);
    }

    [Fact]
    public async Task 正常系_GetActiveConnectionsByMeetingIdAsync_指定した会議のアクティブ接続一覧を取得する()
    {
        // Arrange
        var meetingId = "meeting789";
        var expectedConnections = new List<Connection>
        {
            new Connection(
                new ConnectionId("connection1"),
                new ParticipantId("participant1"),
                new MeetingId(meetingId),
                DateTime.UtcNow,
                true
            ),
            new Connection(
                new ConnectionId("connection2"),
                new ParticipantId("participant2"),
                new MeetingId(meetingId),
                DateTime.UtcNow,
                true
            )
        };

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync(It.IsAny<MeetingId>()))
            .ReturnsAsync(expectedConnections);

        // Act
        var result = await _connectionService.GetActiveConnectionsByMeetingIdAsync(meetingId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, connection => Assert.True(connection.IsActive));
        Assert.All(result, connection => Assert.Equal(meetingId, connection.MeetingId.Value));

        // Repository.GetActiveConnectionsByMeetingIdAsyncが正しい会議IDで呼ばれたことを検証
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync(
            meetingId
        ), Times.Once);
    }

    [Fact]
    public async Task 異常系_ConnectAsync_Repository例外時に適切に再スローされる()
    {
        // Arrange
        var request = new ConnectServiceRequest
        {
            ConnectionId = "connection123",
            ParticipantId = "participant123",
            MeetingId = "meeting123"
        };

        _connectionRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Connection>()))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _connectionService.ConnectAsync(request));

        Assert.Equal("Database connection error", exception.Message);
        _connectionRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Connection>()), Times.Once);
    }

    [Fact]
    public async Task 異常系_DisconnectAsync_Repository例外時に適切に再スローされる()
    {
        // Arrange
        var request = new DisconnectServiceRequest
        {
            ConnectionId = "connection456"
        };

        _connectionRepositoryMock.Setup(x => x.DeactivateAsync(It.IsAny<ConnectionId>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            _connectionService.DisconnectAsync(request));

        Assert.Equal("Connection timeout", exception.Message);
        _connectionRepositoryMock.Verify(x => x.DeactivateAsync(It.IsAny<ConnectionId>()), Times.Once);
    }

    [Fact]
    public async Task 異常系_GetActiveConnectionsByMeetingIdAsync_Repository例外時に適切に再スローされる()
    {
        // Arrange
        var meetingId = "meeting789";

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync(It.IsAny<MeetingId>()))
            .ThrowsAsync(new ArgumentException("Invalid meeting ID"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _connectionService.GetActiveConnectionsByMeetingIdAsync(meetingId));

        Assert.Equal("Invalid meeting ID", exception.Message);
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync(It.IsAny<MeetingId>()), Times.Once);
    }
}