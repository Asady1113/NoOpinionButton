using Core.Domain.Entities;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Message;
using Core.Domain.ValueObjects.Participant;

namespace Core.Tests.Domain.Entities;

public class MessageTests
{
    [Fact]
    public void 正常系_Constructor_全パラメータ指定でMessageエンティティ作成成功()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("テストユーザー");
        var content = new MessageContent("テストメッセージです");
        var createdAt = DateTime.UtcNow;
        var likeCount = new LikeCount(5);
        var reportedCount = new ReportedCount(1);
        var isActive = true;

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content,
            createdAt,
            likeCount,
            reportedCount,
            isActive
        );

        // Assert
        Assert.Equal(messageId, message.Id);
        Assert.Equal(meetingId, message.MeetingId);
        Assert.Equal(participantId, message.ParticipantId);
        Assert.Equal(participantName, message.ParticipantName);
        Assert.Equal(content, message.Content);
        Assert.Equal(createdAt, message.CreatedAt);
        Assert.Equal(likeCount, message.LikeCount);
        Assert.Equal(reportedCount, message.ReportedCount);
        Assert.Equal(isActive, message.IsActive);
    }

    [Fact]
    public void 正常系_Constructor_必須パラメータのみでMessageエンティティ作成成功()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("テストユーザー");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal(messageId, message.Id);
        Assert.Equal(meetingId, message.MeetingId);
        Assert.Equal(participantId, message.ParticipantId);
        Assert.Equal(participantName, message.ParticipantName);
        Assert.Equal(content, message.Content);
        Assert.True(message.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(0, message.LikeCount.Value);
        Assert.Equal(0, message.ReportedCount.Value);
        Assert.True(message.IsActive);
    }

    [Fact]
    public void 正常系_ParticipantName_有効な参加者名でプロパティ設定成功()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("有効なユーザー名");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal("有効なユーザー名", message.ParticipantName.Value);
        Assert.Equal(participantName, message.ParticipantName);
    }

    [Fact]
    public void 正常系_ParticipantName_最大長の参加者名でプロパティ設定成功()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var maxLengthName = new string('あ', ParticipantName.MaxLength); // 50文字
        var participantName = new ParticipantName(maxLengthName);
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal(maxLengthName, message.ParticipantName.Value);
        Assert.Equal(ParticipantName.MaxLength, message.ParticipantName.Value.Length);
    }

    [Fact]
    public void 正常系_ParticipantName_前後の空白が除去されてプロパティ設定成功()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("  テストユーザー  ");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal("テストユーザー", message.ParticipantName.Value);
    }

    [Fact]
    public void 異常系_ParticipantName_空文字列で例外発生()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var content = new MessageContent("テストメッセージです");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Message(
                messageId,
                meetingId,
                participantId,
                new ParticipantName(""),
                content
            )
        );
        Assert.Contains("参加者名は空にできません", exception.Message);
    }

    [Fact]
    public void 異常系_ParticipantName_null文字列で例外発生()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var content = new MessageContent("テストメッセージです");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Message(
                messageId,
                meetingId,
                participantId,
                new ParticipantName(null!),
                content
            )
        );
        Assert.Contains("参加者名は空にできません", exception.Message);
    }

    [Fact]
    public void 異常系_ParticipantName_空白のみで例外発生()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var content = new MessageContent("テストメッセージです");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Message(
                messageId,
                meetingId,
                participantId,
                new ParticipantName("   "),
                content
            )
        );
        Assert.Contains("参加者名は空にできません", exception.Message);
    }

    [Fact]
    public void 異常系_ParticipantName_最大長超過で例外発生()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var content = new MessageContent("テストメッセージです");
        var tooLongName = new string('あ', ParticipantName.MaxLength + 1); // 51文字

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Message(
                messageId,
                meetingId,
                participantId,
                new ParticipantName(tooLongName),
                content
            )
        );
        Assert.Contains($"参加者名は{ParticipantName.MaxLength}文字以内である必要があります", exception.Message);
    }

    [Fact]
    public void 境界値_ParticipantName_1文字の参加者名で正常作成()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("A");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal("A", message.ParticipantName.Value);
    }

    [Fact]
    public void 境界値_ParticipantName_特殊文字を含む参加者名で正常作成()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("テスト@ユーザー#123");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal("テスト@ユーザー#123", message.ParticipantName.Value);
    }

    [Fact]
    public void 境界値_ParticipantName_絵文字を含む参加者名で正常作成()
    {
        // Arrange
        var messageId = new MessageId("msg123");
        var meetingId = new MeetingId("meeting456");
        var participantId = new ParticipantId("participant789");
        var participantName = new ParticipantName("テストユーザー😊");
        var content = new MessageContent("テストメッセージです");

        // Act
        var message = new Message(
            messageId,
            meetingId,
            participantId,
            participantName,
            content
        );

        // Assert
        Assert.Equal("テストユーザー😊", message.ParticipantName.Value);
    }
}