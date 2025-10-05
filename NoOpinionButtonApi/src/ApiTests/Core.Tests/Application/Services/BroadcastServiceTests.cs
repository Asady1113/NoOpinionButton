using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Connection;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;
using Moq;

namespace Core.Tests.Application.Services;

public class BroadcastServiceTests
{
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock;
    private readonly Mock<IBroadcastRepository> _broadcastRepositoryMock;
    private readonly BroadcastService _broadcastService;

    public BroadcastServiceTests()
    {
        _connectionRepositoryMock = new Mock<IConnectionRepository>();
        _broadcastRepositoryMock = new Mock<IBroadcastRepository>();
        _broadcastService = new BroadcastService(_connectionRepositoryMock.Object, _broadcastRepositoryMock.Object);
    }

    /// <summary>
    /// テスト用のConnection作成ヘルパーメソッド
    /// </summary>
    private static Connection CreateTestConnection(string connectionId, string participantId, string meetingId, bool isActive = true)
    {
        return new Connection(
            new ConnectionId(connectionId),
            new ParticipantId(participantId),
            new MeetingId(meetingId),
            DateTime.UtcNow,
            isActive
        );
    }

    [Fact]
    public async Task 正常系_BroadcastMessageToMeetingAsync_指定した会議の全接続にメッセージが配信される()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\",\"type\":\"message\"}";
        
        var activeConnections = new List<Connection>
        {
            CreateTestConnection("connection1", "participant1", "meeting123"),
            CreateTestConnection("connection2", "participant2", "meeting123")
        };

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"))
            .ReturnsAsync(activeConnections);

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), messageJson))
            .ReturnsAsync(2); // 2つの接続に配信成功

        // Act
        await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);

        // Assert
        // ConnectionRepositoryから有効な接続を取得することを検証
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"), Times.Once);

        // BroadcastRepositoryに正しい接続IDリストとメッセージで配信が実行されることを検証
        var expectedConnectionIds = new List<ConnectionId> { "connection1", "connection2" };
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(
            It.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConnectionIds)),
            messageJson
        ), Times.Once);
    }

    [Fact]
    public async Task 異常系_BroadcastMessageToMeetingAsync_ConnectionRepository例外時に適切に再スローされる()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\",\"type\":\"message\"}";

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson));
        
        Assert.Equal("Database error", exception.Message);
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"), Times.Once);
        
        // BroadcastRepositoryは呼ばれないことを検証
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_BroadcastMessageToMeetingAsync_BroadcastRepository例外時に適切に再スローされる()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\",\"type\":\"message\"}";
        
        var activeConnections = new List<Connection>
        {
            CreateTestConnection("connection1", "participant1", "meeting123")
        };

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"))
            .ReturnsAsync(activeConnections);

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), messageJson))
            .ThrowsAsync(new TimeoutException("Broadcast timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson));
        
        Assert.Equal("Broadcast timeout", exception.Message);
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"), Times.Once);
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), messageJson), Times.Once);
    }

    [Fact]
    public async Task 正常系_BroadcastMessageToMeetingAsync_接続が存在しない場合は空のIDリストで配信処理を呼ぶ()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\"}";
        
        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync(meetingId))
            .ReturnsAsync(new List<Connection>());

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), messageJson))
            .ReturnsAsync(0);

        // Act
        await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);

        // Assert
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync(meetingId), Times.Once);
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(
            It.Is<IEnumerable<ConnectionId>>(ids => !ids.Any()), messageJson), Times.Once);
    }

    [Fact]
    public async Task 正常系_BroadcastMessageToMeetingAsync_大量の接続でも正常に配信処理()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\"}";
        
        var activeConnections = Enumerable.Range(1, 100)
            .Select(i => CreateTestConnection($"connection{i}", $"participant{i}", meetingId))
            .ToList();

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync(meetingId))
            .ReturnsAsync(activeConnections);

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<ConnectionId>>(), messageJson))
            .ReturnsAsync(100);

        // Act
        await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);

        // Assert
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync(meetingId), Times.Once);
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(
            It.Is<IEnumerable<ConnectionId>>(ids => ids.Count() == 100), 
            messageJson), Times.Once);
    }
}