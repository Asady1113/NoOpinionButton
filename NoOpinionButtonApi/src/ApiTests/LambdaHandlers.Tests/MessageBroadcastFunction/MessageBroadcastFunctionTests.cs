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
    private Dictionary<string, DynamoDBEvent.AttributeValue> CreateDefaultMessageAttributes()
    {
        return new Dictionary<string, DynamoDBEvent.AttributeValue>
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
                json.Contains("participant-789"))),
            Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Processing record: INSERT", logMessages);
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456", logMessages);
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
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456", logMessages);
        Assert.Contains("Broadcasting message message-999 to meeting meeting-888", logMessages);
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
        Assert.Contains("Broadcasting message message-123 to meeting meeting-456", logMessages);
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
        Assert.Contains("Broadcasting message message-long to meeting meeting-456", logMessages);
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
}