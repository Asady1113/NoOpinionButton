using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Moq;

namespace LambdaHandlers.Tests.WebSocketConnectFunction;

public class WebSocketConnectFunctionTests
{
    private readonly Mock<IConnectionService> _connectionServiceMock;
    private readonly global::WebSocketConnectFunction.Function _function;
    private readonly TestLambdaContext _context;

    public WebSocketConnectFunctionTests()
    {
        _connectionServiceMock = new Mock<IConnectionService>();
        _context = new TestLambdaContext();
        
        // テスト専用DIコンテナを作成してモックを注入
        _function = CreateFunctionWithMockedService();
    }

    private global::WebSocketConnectFunction.Function CreateFunctionWithMockedService()
    {
        // 通常のコンストラクタでFunctionを作成
        var function = new global::WebSocketConnectFunction.Function();
        
        // リフレクションで_connectionServiceフィールドをモックに置き換え
        var connectionServiceField = typeof(global::WebSocketConnectFunction.Function)
            .GetField("_connectionService", BindingFlags.NonPublic | BindingFlags.Instance);
        connectionServiceField?.SetValue(function, _connectionServiceMock.Object);
        
        return function;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_有効な接続リクエスト_接続成功()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-123",
                RouteKey = "$connect"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "meeting456",
                ["participantId"] = "participant789"
            }
        };

        var serviceResponse = new ConnectServiceResponse
        {
            ConnectionId = "test-connection-123"
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Connected", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.Is<ConnectServiceRequest>(r =>
            r.ConnectionId == "test-connection-123" &&
            r.MeetingId == "meeting456" &&
            r.ParticipantId == "participant789")), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Connect - ConnectionId: test-connection-123", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_必須パラメータのみ_接続成功()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-456"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "meeting123"
            }
        };

        var serviceResponse = new ConnectServiceResponse
        {
            ConnectionId = "test-connection-456"
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Connected", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.Is<ConnectServiceRequest>(r =>
            r.ConnectionId == "test-connection-456" &&
            r.MeetingId == "meeting123" &&
            r.ParticipantId == "")), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_クエリパラメータなし_空文字で接続処理()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-789"
            },
            QueryStringParameters = null
        };

        var serviceResponse = new ConnectServiceResponse
        {
            ConnectionId = "test-connection-789"
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Connected", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.Is<ConnectServiceRequest>(r =>
            r.ConnectionId == "test-connection-789" &&
            r.MeetingId == "" &&
            r.ParticipantId == "")), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_空のクエリパラメータ_空文字で接続処理()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-empty"
            },
            QueryStringParameters = new Dictionary<string, string>()
        };

        var serviceResponse = new ConnectServiceResponse
        {
            ConnectionId = "test-connection-empty"
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Connected", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_長いパラメータ値_正常処理()
    {
        // Arrange
        var longMeetingId = new string('m', 255); // 255文字の会議ID
        var longParticipantId = new string('p', 255); // 255文字の参加者ID
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-long"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = longMeetingId,
                ["participantId"] = longParticipantId
            }
        };

        var serviceResponse = new ConnectServiceResponse
        {
            ConnectionId = "test-connection-long"
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Connected", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.Is<ConnectServiceRequest>(r =>
            r.ConnectionId == "test-connection-long" &&
            r.MeetingId == longMeetingId &&
            r.ParticipantId == longParticipantId)), Times.Once);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_ConnectionService例外_UnauthorizedAccess_500エラー()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-unauthorized"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "meeting123"
            }
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Meeting access denied"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Internal Server Error", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Connect - ConnectionId: test-connection-unauthorized", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("UnauthorizedAccessException", logMessages);
        Assert.Contains("Meeting access denied", logMessages);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_ConnectionService例外_KeyNotFound_500エラー()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-notfound"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "nonexistent-meeting"
            }
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ThrowsAsync(new KeyNotFoundException("Meeting not found"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Internal Server Error", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Connect - ConnectionId: test-connection-notfound", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("KeyNotFoundException", logMessages);
        Assert.Contains("Meeting not found", logMessages);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_ConnectionService例外_ArgumentException_500エラー()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-invalid"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "invalid-meeting-format"
            }
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ThrowsAsync(new ArgumentException("Invalid meeting ID format"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Internal Server Error", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Connect - ConnectionId: test-connection-invalid", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("ArgumentException", logMessages);
        Assert.Contains("Invalid meeting ID format", logMessages);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_予期しない例外_500エラー()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-unexpected"
            },
            QueryStringParameters = new Dictionary<string, string>
            {
                ["meetingId"] = "meeting123"
            }
        };

        _connectionServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected database error"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Internal Server Error", response.Body);

        _connectionServiceMock.Verify(x => x.ConnectAsync(It.IsAny<ConnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Connect - ConnectionId: test-connection-unexpected", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("InvalidOperationException", logMessages);
        Assert.Contains("Unexpected database error", logMessages);
    }
}