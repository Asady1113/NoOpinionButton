using System.Reflection;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application.Ports;
using Moq;

namespace LambdaHandlers.Tests.MessageBroadcastFunction;

public class MessageBroadcastFunctionTests
{
    private readonly Mock<IBroadcastService> _broadcastServiceMock;
    private readonly global::MessageBroadcastFunction.Function _function;
    private readonly TestLambdaContext _context;

    public MessageBroadcastFunctionTests()
    {
        // テスト用環境変数を設定
        Environment.SetEnvironmentVariable("WEBSOCKET_API_ENDPOINT", "https://test.execute-api.region.amazonaws.com/stage");
        
        _broadcastServiceMock = new Mock<IBroadcastService>();
        _context = new TestLambdaContext();
        
        // テスト専用DIコンテナを作成してモックを注入
        _function = CreateFunctionWithMockedService();
    }

    private global::MessageBroadcastFunction.Function CreateFunctionWithMockedService()
    {
        // 通常のコンストラクタでFunctionを作成
        var function = new global::MessageBroadcastFunction.Function();
        
        // リフレクションで_broadcastServiceフィールドをモックに置き換え
        var broadcastServiceField = typeof(global::MessageBroadcastFunction.Function)
            .GetField("_broadcastService", BindingFlags.NonPublic | BindingFlags.Instance);
        broadcastServiceField?.SetValue(function, _broadcastServiceMock.Object);
        
        return function;
    }

    /// <summary>
    /// テスト用DynamoDBEventを作成する
    /// </summary>
    private DynamoDBEvent CreateDynamoDBEvent(string eventName, Dictionary<string, DynamoDBEvent.AttributeValue>? newImage = null)
    {
        return new DynamoDBEvent
        {
            Records = new List<DynamoDBEvent.DynamodbStreamRecord>
            {
                new()
                {
                    EventName = eventName,
                    Dynamodb = new DynamoDBEvent.StreamRecord
                    {
                        NewImage = newImage ?? CreateDefaultMessageAttributes()
                    }
                }
            }
        };
    }

    /// <summary>
    /// テスト用のデフォルトメッセージ属性を作成する
    /// </summary>
    private Dictionary<string, DynamoDBEvent.AttributeValue> CreateDefaultMessageAttributes(bool includeParticipantName = true)
    {
        var attributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-123" },
            ["MeetingId"] = new() { S = "meeting-456" },
            ["ParticipantId"] = new() { S = "participant-789" },
            ["Content"] = new() { S = "テストメッセージです。" },
            ["CreatedAt"] = new() { S = "2025-01-15T10:30:00Z" },
            ["LikeCount"] = new() { N = "0" },
            ["ReportedCount"] = new() { N = "0" },
            ["IsActive"] = new() { N = "1" }
        };

        if (includeParticipantName)
        {
            attributes["ParticipantName"] = new() { S = "テストユーザー" };
        }

        return attributes;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_INSERTイベント_メッセージ配信成功()
    {
        // Arrange
        var dynamoEvent = CreateDynamoDBEvent("INSERT");

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("message-123") && 
                json.Contains("meeting-456") &&
                json.Contains("participant-789") &&
                json.Contains("participantName"))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: INSERT", logMessages);
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant テストユーザー", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_MODIFYイベント_処理スキップ()
    {
        // Arrange
        var dynamoEvent = CreateDynamoDBEvent("MODIFY");

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: MODIFY", logMessages);
        Assert.DoesNotContain("Broadcasting message", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_REMOVEイベント_処理スキップ()
    {
        // Arrange
        var dynamoEvent = CreateDynamoDBEvent("REMOVE");

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: REMOVE", logMessages);
        Assert.DoesNotContain("Broadcasting message", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_複数レコード_INSERTのみ処理()
    {
        // Arrange
        var dynamoEvent = new DynamoDBEvent
        {
            Records = new List<DynamoDBEvent.DynamodbStreamRecord>
            {
                new()
                {
                    EventName = "INSERT",
                    Dynamodb = new DynamoDBEvent.StreamRecord
                    {
                        NewImage = CreateDefaultMessageAttributes()
                    }
                },
                new()
                {
                    EventName = "MODIFY",
                    Dynamodb = new DynamoDBEvent.StreamRecord
                    {
                        NewImage = CreateDefaultMessageAttributes()
                    }
                },
                new()
                {
                    EventName = "INSERT",
                    Dynamodb = new DynamoDBEvent.StreamRecord
                    {
                        NewImage = new Dictionary<string, DynamoDBEvent.AttributeValue>
                        {
                            ["Id"] = new() { S = "message-999" },
                            ["MeetingId"] = new() { S = "meeting-888" },
                            ["ParticipantId"] = new() { S = "participant-777" },
                            ["ParticipantName"] = new() { S = "二番目のユーザー" },
                            ["Content"] = new() { S = "二番目のメッセージです。" },
                            ["CreatedAt"] = new() { S = "2025-01-15T10:35:00Z" },
                            ["LikeCount"] = new() { N = "0" },
                            ["ReportedCount"] = new() { N = "0" },
                            ["IsActive"] = new() { N = "1" }
                        }
                    }
                }
            }
        };

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", It.IsAny<string>()), Times.Once);
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-888", It.IsAny<string>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: INSERT", logMessages);
        Assert.Contains("Processing record: MODIFY", logMessages);
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant テストユーザー", logMessages);
        Assert.Contains("Broadcasting message message-999 to meeting meeting-888 from participant 二番目のユーザー", logMessages);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_BroadcastService例外_例外を再スロー()
    {
        // Arrange
        var dynamoEvent = CreateDynamoDBEvent("INSERT");

        var expectedException = new InvalidOperationException("Broadcast service error");
        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _function.FunctionHandler(dynamoEvent, _context));

        Assert.Equal("Broadcast service error", actualException.Message);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: INSERT", logMessages);
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant テストユーザー", logMessages);
        Assert.Contains("Error processing message insert:", logMessages);
        Assert.Contains("InvalidOperationException", logMessages);
        Assert.Contains("Broadcast service error", logMessages);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_不正なDynamoDBデータ_例外を再スロー()
    {
        // Arrange
        var invalidAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-123" },
            ["MeetingId"] = new() { S = "meeting-456" },
            // 必須フィールドが欠如している不正なデータ
            ["Content"] = new() { S = "テストメッセージ" }
            // ParticipantId, CreatedAt, LikeCount, ReportedCount, IsActive が欠如
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", invalidAttributes);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _function.FunctionHandler(dynamoEvent, _context));

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: INSERT", logMessages);
        Assert.Contains("Error processing message insert:", logMessages);
        Assert.Contains("KeyNotFoundException", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_長いメッセージ内容_500文字以内で正常処理()
    {
        // Arrange
        var longContent = new string('あ', 500); // 500文字のメッセージ
        var messageAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-long" },
            ["MeetingId"] = new() { S = "meeting-456" },
            ["ParticipantId"] = new() { S = "participant-789" },
            ["ParticipantName"] = new() { S = "長文ユーザー" },
            ["Content"] = new() { S = longContent },
            ["CreatedAt"] = new() { S = "2025-01-15T10:30:00Z" },
            ["LikeCount"] = new() { N = "0" },
            ["ReportedCount"] = new() { N = "0" },
            ["IsActive"] = new() { N = "1" }
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => json.Contains("message-long"))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-long to meeting meeting-456 from participant 長文ユーザー", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_数値フィールドの境界値_正常処理()
    {
        // Arrange
        var messageAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-boundary" },
            ["MeetingId"] = new() { S = "meeting-456" },
            ["ParticipantId"] = new() { S = "participant-789" },
            ["ParticipantName"] = new() { S = "境界値ユーザー" },
            ["Content"] = new() { S = "境界値テストメッセージ" },
            ["CreatedAt"] = new() { S = "2025-01-15T10:30:00Z" },
            ["LikeCount"] = new() { N = "2147483647" }, // int.MaxValue
            ["ReportedCount"] = new() { N = "0" },
            ["IsActive"] = new() { N = "0" } // 非アクティブ状態
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("2147483647") && 
                json.Contains("\"isActive\":0"))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantName含む_JSON配信成功()
    {
        // Arrange
        var messageAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-with-name" },
            ["MeetingId"] = new() { S = "meeting-123" },
            ["ParticipantId"] = new() { S = "participant-456" },
            ["ParticipantName"] = new() { S = "山田太郎" },
            ["Content"] = new() { S = "参加者名付きメッセージ" },
            ["CreatedAt"] = new() { S = "2025-01-15T11:00:00Z" },
            ["LikeCount"] = new() { N = "5" },
            ["ReportedCount"] = new() { N = "1" },
            ["IsActive"] = new() { N = "1" }
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-123", 
            It.Is<string>(json => 
                json.Contains("\"messageId\":\"message-with-name\"") &&
                json.Contains("\"meetingId\":\"meeting-123\"") &&
                json.Contains("\"participantId\":\"participant-456\"") &&
                json.Contains("\"participantName\"") &&
                json.Contains("\"likeCount\":5") &&
                json.Contains("\"reportedCount\":1") &&
                json.Contains("\"isActive\":1"))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-with-name to meeting meeting-123 from participant 山田太郎", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantName存在しない_フォールバック処理()
    {
        // Arrange
        var messageAttributes = CreateDefaultMessageAttributes(includeParticipantName: false);
        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("\"participantName\"") &&
                json.Contains("\"messageId\":\"message-123\"") &&
                json.Contains("\"participantId\":\"participant-789\""))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant 不明なユーザー", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantNameが空文字列_フォールバック処理()
    {
        // Arrange
        var messageAttributes = CreateDefaultMessageAttributes();
        messageAttributes["ParticipantName"] = new DynamoDBEvent.AttributeValue { S = "" };
        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("\"participantName\":\"\"") &&
                json.Contains("\"messageId\":\"message-123\""))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantNameがnull_フォールバック処理()
    {
        // Arrange
        var messageAttributes = CreateDefaultMessageAttributes();
        messageAttributes["ParticipantName"] = new DynamoDBEvent.AttributeValue { NULL = true };
        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("\"participantName\"") &&
                json.Contains("\"messageId\":\"message-123\""))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456 from participant 不明なユーザー", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantName特殊文字_正常処理()
    {
        // Arrange
        var messageAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-special" },
            ["MeetingId"] = new() { S = "meeting-456" },
            ["ParticipantId"] = new() { S = "participant-789" },
            ["ParticipantName"] = new() { S = "田中@花子_123" },
            ["Content"] = new() { S = "特殊文字テスト" },
            ["CreatedAt"] = new() { S = "2025-01-15T12:00:00Z" },
            ["LikeCount"] = new() { N = "0" },
            ["ReportedCount"] = new() { N = "0" },
            ["IsActive"] = new() { N = "1" }
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("\"participantName\"") &&
                json.Contains("\"messageId\":\"message-special\""))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Broadcasting message message-special to meeting meeting-456 from participant 田中@花子_123", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ParticipantName最大長_正常処理()
    {
        // Arrange
        var longParticipantName = new string('あ', 50); // 50文字の参加者名
        var messageAttributes = new Dictionary<string, DynamoDBEvent.AttributeValue>
        {
            ["Id"] = new() { S = "message-long-name" },
            ["MeetingId"] = new() { S = "meeting-456" },
            ["ParticipantId"] = new() { S = "participant-789" },
            ["ParticipantName"] = new() { S = longParticipantName },
            ["Content"] = new() { S = "長い名前テスト" },
            ["CreatedAt"] = new() { S = "2025-01-15T13:00:00Z" },
            ["LikeCount"] = new() { N = "0" },
            ["ReportedCount"] = new() { N = "0" },
            ["IsActive"] = new() { N = "1" }
        };

        var dynamoEvent = CreateDynamoDBEvent("INSERT", messageAttributes);

        _broadcastServiceMock
            .Setup(x => x.BroadcastMessageToMeetingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.FunctionHandler(dynamoEvent, _context);

        // Assert
        _broadcastServiceMock.Verify(x => x.BroadcastMessageToMeetingAsync(
            "meeting-456", 
            It.Is<string>(json => 
                json.Contains("\"participantName\"") &&
                json.Contains("\"messageId\":\"message-long-name\""))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains($"Broadcasting message message-long-name to meeting meeting-456 from participant {longParticipantName}", logMessages);
        Assert.Contains("Message broadcast completed", logMessages);
    }
}