using Core.Application.DTOs.Requests;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Moq;

namespace Core.Tests.Application.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _messageService = new MessageService(_messageRepositoryMock.Object);
    }

    [Fact]
    public async Task 正常系_PostMessageAsync_メッセージ送信処理が成功()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123",
            ParticipantId = "participant123",
            Content = "テストメッセージ"
        };

        var savedMessage = new Message(
            "message123",
            "meeting123", 
            "participant123",
            "テストメッセージ",
            DateTime.UtcNow
        );

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _messageService.PostMessageAsync(request);

        // Assert
        Assert.Equal("message123", result.MessageId);
    }

    [Fact]
    public async Task 正常系_PostMessageAsync_ServiceRequestからEntityへの変換が正しく実行される()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting456",
            ParticipantId = "participant456", 
            Content = "変換テストメッセージ"
        };

        var savedMessage = new Message(
            "message456",
            "meeting456",
            "participant456", 
            "変換テストメッセージ",
            DateTime.UtcNow
        );

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _messageService.PostMessageAsync(request);

        // Assert
        Assert.Equal("message456", result.MessageId);

        // Repository.SaveAsyncが正しいプロパティを持つMessageエンティティで呼ばれたことを検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.Is<Message>(m => 
            m.MeetingId.Value == "meeting456" &&
            m.ParticipantId.Value == "participant456" &&
            m.Content.Value == "変換テストメッセージ" &&
            !string.IsNullOrEmpty(m.Id.Value)
        )), Times.Once);
    }

    [Fact] 
    public async Task 正常系_PostMessageAsync_Repository_SaveAsyncが正しい引数で呼ばれることを検証()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting789",
            ParticipantId = "participant789",
            Content = "引数検証テストメッセージ"
        };

        var savedMessage = new Message(
            "message789",
            "meeting789",
            "participant789",
            "引数検証テストメッセージ",
            DateTime.UtcNow
        );

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        await _messageService.PostMessageAsync(request);

        // Assert
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async Task 異常系_PostMessageAsync_Repository例外時に適切に再スロー()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123",
            ParticipantId = "participant123", 
            Content = "例外テストメッセージ"
        };

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _messageService.PostMessageAsync(request));
        
        Assert.Equal("Database error", exception.Message);
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact] 
    public async Task 異常系_PostMessageAsync_null引数で例外処理()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => 
            _messageService.PostMessageAsync(null!));

        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task 正常系_PostMessageAsync_メッセージ作成時にGUID生成とUTC時刻設定が実行される()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting999",
            ParticipantId = "participant999",
            Content = "GUID生成テストメッセージ"
        };

        var savedMessage = new Message(
            "generated-guid-123",
            "meeting999",
            "participant999",
            "GUID生成テストメッセージ",
            DateTime.UtcNow
        );

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _messageService.PostMessageAsync(request);

        // Assert  
        Assert.Equal("generated-guid-123", result.MessageId);

        // MessageエンティティがGUIDと現在時刻で作成されることを検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.Is<Message>(m => 
            !string.IsNullOrEmpty(m.Id.Value) &&
            m.CreatedAt <= DateTime.UtcNow &&
            m.CreatedAt >= DateTime.UtcNow.AddSeconds(-5) // 5秒以内の時刻
        )), Times.Once);
    }

    [Fact]
    public async Task 異常系_PostMessageAsync_Repository例外の種類が保持される()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123", 
            ParticipantId = "participant123",
            Content = "例外種類テストメッセージ"
        };

        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _messageService.PostMessageAsync(request));
        
        Assert.Equal("Connection timeout", exception.Message);
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Once);
    }
}