using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
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

    [Fact]
    public async Task 正常系_BroadcastMessageToMeetingAsync_指定した会議の全接続にメッセージが配信される()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\",\"type\":\"message\"}";
        
        var activeConnections = new List<Connection>
        {
            new Connection("connection1", "participant1", "meeting123", DateTime.UtcNow, true),
            new Connection("connection2", "participant2", "meeting123", DateTime.UtcNow, true)
        };

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"))
            .ReturnsAsync(activeConnections);

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<string>>(), messageJson))
            .ReturnsAsync(2); // 2つの接続に配信成功

        // Act
        await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);

        // Assert
        // ConnectionRepositoryから有効な接続を取得することを検証
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"), Times.Once);

        // BroadcastRepositoryに正しい接続IDリストとメッセージで配信が実行されることを検証
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(
            It.Is<IEnumerable<string>>(connectionIds => 
                connectionIds.Contains("connection1") && 
                connectionIds.Contains("connection2") &&
                connectionIds.Count() == 2
            ), 
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
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_BroadcastMessageToMeetingAsync_BroadcastRepository例外時に適切に再スローされる()
    {
        // Arrange
        var meetingId = "meeting123";
        var messageJson = "{\"content\":\"テストメッセージ\",\"type\":\"message\"}";
        
        var activeConnections = new List<Connection>
        {
            new Connection("connection1", "participant1", "meeting123", DateTime.UtcNow, true)
        };

        _connectionRepositoryMock.Setup(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"))
            .ReturnsAsync(activeConnections);

        _broadcastRepositoryMock.Setup(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<string>>(), messageJson))
            .ThrowsAsync(new TimeoutException("Broadcast timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson));
        
        Assert.Equal("Broadcast timeout", exception.Message);
        _connectionRepositoryMock.Verify(x => x.GetActiveConnectionsByMeetingIdAsync("meeting123"), Times.Once);
        _broadcastRepositoryMock.Verify(x => x.BroadcastToMultipleConnectionsAsync(It.IsAny<IEnumerable<string>>(), messageJson), Times.Once);
    }
}