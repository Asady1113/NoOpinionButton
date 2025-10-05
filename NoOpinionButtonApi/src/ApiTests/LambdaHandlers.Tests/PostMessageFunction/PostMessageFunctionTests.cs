using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Moq;
using PostMessageFunction.DTOs;
using Common.ApiResponse;

namespace LambdaHandlers.Tests.PostMessageFunction;

public class PostMessageFunctionTests
{
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly global::PostMessageFunction.Function _function;
    private readonly TestLambdaContext _context;

    public PostMessageFunctionTests()
    {
        _messageServiceMock = new Mock<IMessageService>();
        _context = new TestLambdaContext();
        
        // テスト専用DIコンテナを作成してモックを注入
        _function = CreateFunctionWithMockedService();
    }

    private global::PostMessageFunction.Function CreateFunctionWithMockedService()
    {
        // 通常のコンストラクタでFunctionを作成
        var function = new global::PostMessageFunction.Function();
        
        // リフレクションで_messageServiceフィールドをモックに置き換え
        var messageServiceField = typeof(global::PostMessageFunction.Function)
            .GetField("_messageService", BindingFlags.NonPublic | BindingFlags.Instance);
        messageServiceField?.SetValue(function, _messageServiceMock.Object);
        
        return function;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_POST_message_有効なリクエストで正常レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "participant456", 
                content = "テストメッセージです。" 
            })
        };

        var serviceResponse = new PostMessageServiceResponse
        {
            MessageId = "message789"
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PostMessageResponse>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("message789", apiResponse.Data.MessageId);

        _messageServiceMock.Verify(x => x.PostMessageAsync(It.Is<PostMessageServiceRequest>(r =>
            r.MeetingId == "meeting123" && 
            r.ParticipantId == "participant456" && 
            r.Content == "テストメッセージです。")), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_OPTIONS_message_CORS正常レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "OPTIONS"
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.True(response.Headers.ContainsKey("Access-Control-Allow-Origin"));
        Assert.True(response.Headers.ContainsKey("Access-Control-Allow-Methods"));
        Assert.True(response.Headers.ContainsKey("Access-Control-Allow-Headers"));
    }

    [Fact]
    public async Task 異常系_FunctionHandler_無効なHTTPメソッド_NotFound()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "GET"
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Requested endpoint was not found", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_無効なパス_NotFound()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/invalid",
            HttpMethod = "POST"
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Requested endpoint was not found", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_空のリクエストボディ_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = null
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Missing request body", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_不正なJSON形式_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = "{ invalid json"
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Invalid JSON format in request body", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_必須フィールド欠如_meetingId_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                participantId = "participant456", 
                content = "テストメッセージです。" 
            }) // meetingIdが欠如
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("'meetingId', 'participantId', and 'content' are required", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_必須フィールド欠如_participantId_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                content = "テストメッセージです。" 
            }) // participantIdが欠如
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("'meetingId', 'participantId', and 'content' are required", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_必須フィールド欠如_content_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "participant456" 
            }) // contentが欠如
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("'meetingId', 'participantId', and 'content' are required", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_認証失敗_Unauthorized()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "invalidParticipant", 
                content = "テストメッセージです。" 
            })
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid participant credentials"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(401, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Invalid participant credentials", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_会議が見つからない_NotFound()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "nonexistent", 
                participantId = "participant456", 
                content = "テストメッセージです。" 
            })
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ThrowsAsync(new KeyNotFoundException("Meeting not found"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Meeting not found", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_予期しない例外_ServerError()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "participant456", 
                content = "テストメッセージです。" 
            })
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Internal server error", apiResponse.Error);
        
        // ログ出力の確認（TestLambdaContextのログが出力されているかチェック）
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("InvalidOperationException", logMessages);
        Assert.Contains("Database connection failed", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_長いメッセージ内容_500文字以内で正常処理()
    {
        // Arrange
        var longContent = new string('あ', 500); // 500文字のメッセージ
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "participant456", 
                content = longContent 
            })
        };

        var serviceResponse = new PostMessageServiceResponse
        {
            MessageId = "message-long-content"
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PostMessageResponse>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("message-long-content", apiResponse.Data.MessageId);

        _messageServiceMock.Verify(x => x.PostMessageAsync(It.Is<PostMessageServiceRequest>(r =>
            r.MeetingId == "meeting123" && 
            r.ParticipantId == "participant456" && 
            r.Content == longContent)), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_特殊文字を含むメッセージ_正常処理()
    {
        // Arrange
        var specialContent = "特殊文字テスト: !@#$%^&*()_+-=[]{}|;:,.<>? 絵文字🎉🚀💯 改行\n含む";
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "meeting123", 
                participantId = "participant456", 
                content = specialContent 
            })
        };

        var serviceResponse = new PostMessageServiceResponse
        {
            MessageId = "message-special-chars"
        };

        _messageServiceMock
            .Setup(x => x.PostMessageAsync(It.IsAny<PostMessageServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PostMessageResponse>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("message-special-chars", apiResponse.Data.MessageId);

        _messageServiceMock.Verify(x => x.PostMessageAsync(It.Is<PostMessageServiceRequest>(r =>
            r.MeetingId == "meeting123" && 
            r.ParticipantId == "participant456" && 
            r.Content == specialContent)), Times.Once);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_空文字列フィールド_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/message",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { 
                meetingId = "", 
                participantId = "participant456", 
                content = "テストメッセージです。" 
            }) // meetingIdが空文字列
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("'meetingId', 'participantId', and 'content' are required", apiResponse.Error);
    }
}