using Core.Application.DTOs.Requests;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;
using Moq;

namespace Core.Tests.Application.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IParticipantRepository> _participantRepositoryMock;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _participantRepositoryMock = new Mock<IParticipantRepository>();
        _messageService = new MessageService(_messageRepositoryMock.Object, _participantRepositoryMock.Object);
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

        var participant = new Participant(
            new ParticipantId("participant123"),
            new ParticipantName("テストユーザー"),
            new MeetingId("meeting123"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "message123",
            "meeting123", 
            "participant123",
            "テストユーザー",
            "テストメッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant123")))
            .ReturnsAsync(participant);
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

        var participant = new Participant(
            new ParticipantId("participant456"),
            new ParticipantName("変換テストユーザー"),
            new MeetingId("meeting456"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "message456",
            "meeting456",
            "participant456",
            "変換テストユーザー",
            "変換テストメッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant456")))
            .ReturnsAsync(participant);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _messageService.PostMessageAsync(request);

        // Assert
        Assert.Equal("message456", result.MessageId);

        // Repository.SaveAsyncが正しいプロパティを持つMessageエンティティで呼ばれたことを検証（ParticipantNameを含む）
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.Is<Message>(m => 
            m.MeetingId.Value == "meeting456" &&
            m.ParticipantId.Value == "participant456" &&
            m.ParticipantName.Value == "変換テストユーザー" &&
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

        var participant = new Participant(
            new ParticipantId("participant789"),
            new ParticipantName("引数検証ユーザー"),
            new MeetingId("meeting789"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "message789",
            "meeting789",
            "participant789",
            "引数検証ユーザー",
            "引数検証テストメッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant789")))
            .ReturnsAsync(participant);
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

        var participant = new Participant(
            new ParticipantId("participant123"),
            new ParticipantName("例外テストユーザー"),
            new MeetingId("meeting123"),
            new NoOpinionPoint(0)
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant123")))
            .ReturnsAsync(participant);
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

        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
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

        var participant = new Participant(
            new ParticipantId("participant999"),
            new ParticipantName("GUID生成ユーザー"),
            new MeetingId("meeting999"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "generated-guid-123",
            "meeting999",
            "participant999",
            "GUID生成ユーザー",
            "GUID生成テストメッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant999")))
            .ReturnsAsync(participant);
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

        var participant = new Participant(
            new ParticipantId("participant123"),
            new ParticipantName("例外種類テストユーザー"),
            new MeetingId("meeting123"),
            new NoOpinionPoint(0)
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant123")))
            .ReturnsAsync(participant);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _messageService.PostMessageAsync(request));
        
        Assert.Equal("Connection timeout", exception.Message);
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async Task 正常系_PostMessageAsync_参加者が存在する場合_ParticipantNameが正しく設定される()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123",
            ParticipantId = "participant123",
            Content = "参加者名テストメッセージ"
        };

        var participant = new Participant(
            new ParticipantId("participant123"),
            new ParticipantName("テスト参加者"),
            new MeetingId("meeting123"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "message123",
            "meeting123",
            "participant123",
            "テスト参加者",
            "参加者名テストメッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant123")))
            .ReturnsAsync(participant);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _messageService.PostMessageAsync(request);

        // Assert
        Assert.Equal("message123", result.MessageId);
        
        // ParticipantRepositoryが正しく呼ばれることを検証
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(new ParticipantId("participant123")), Times.Once);
        
        // MessageRepositoryのSaveAsyncでParticipantNameが正しく設定されることを検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.Is<Message>(m => 
            m.ParticipantName.Value == "テスト参加者"
        )), Times.Once);
    }

    [Fact]
    public async Task 異常系_PostMessageAsync_参加者が存在しない場合_ArgumentExceptionをスロー()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123",
            ParticipantId = "nonexistent-participant",
            Content = "存在しない参加者テストメッセージ"
        };

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("nonexistent-participant")))
            .ReturnsAsync((Participant?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _messageService.PostMessageAsync(request));
        
        Assert.Contains("参加者が見つかりません: nonexistent-participant", exception.Message);
        
        // ParticipantRepositoryが呼ばれることを検証
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(new ParticipantId("nonexistent-participant")), Times.Once);
        
        // MessageRepositoryのSaveAsyncは呼ばれないことを検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task 正常系_PostMessageAsync_MessageRepositoryのSaveAsyncでParticipantNameを含むMessageが渡される()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting456",
            ParticipantId = "participant456",
            Content = "ParticipantName検証メッセージ"
        };

        var participant = new Participant(
            new ParticipantId("participant456"),
            new ParticipantName("検証用参加者"),
            new MeetingId("meeting456"),
            new NoOpinionPoint(0)
        );

        var savedMessage = new Message(
            "message456",
            "meeting456",
            "participant456",
            "検証用参加者",
            "ParticipantName検証メッセージ",
            DateTime.UtcNow
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant456")))
            .ReturnsAsync(participant);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        await _messageService.PostMessageAsync(request);

        // Assert
        // MessageRepositoryのSaveAsyncが正しいParticipantNameを持つMessageで呼ばれることを詳細に検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.Is<Message>(m => 
            m.MeetingId.Value == "meeting456" &&
            m.ParticipantId.Value == "participant456" &&
            m.ParticipantName.Value == "検証用参加者" &&
            m.Content.Value == "ParticipantName検証メッセージ" &&
            !string.IsNullOrEmpty(m.Id.Value) &&
            m.CreatedAt <= DateTime.UtcNow &&
            m.CreatedAt >= DateTime.UtcNow.AddSeconds(-5)
        )), Times.Once);
    }

    [Fact]
    public async Task 異常系_PostMessageAsync_ParticipantRepository例外時に適切に再スロー()
    {
        // Arrange
        var request = new PostMessageServiceRequest
        {
            MeetingId = "meeting123",
            ParticipantId = "participant123",
            Content = "ParticipantRepository例外テストメッセージ"
        };

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(new ParticipantId("participant123")))
            .ThrowsAsync(new InvalidOperationException("Participant database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _messageService.PostMessageAsync(request));
        
        Assert.Equal("Participant database error", exception.Message);
        
        // ParticipantRepositoryが呼ばれることを検証
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(new ParticipantId("participant123")), Times.Once);
        
        // MessageRepositoryのSaveAsyncは呼ばれないことを検証
        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Message>()), Times.Never);
    }
}