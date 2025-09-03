using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application.Ports;
using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Moq;

namespace LambdaHandlers.Tests.WebSocketDisconnectFunction;

public class WebSocketDisconnectFunctionTests
{
    private readonly Mock<IConnectionService> _connectionServiceMock;
    private readonly global::WebSocketDisconnectFunction.Function _function;
    private readonly TestLambdaContext _context;

    public WebSocketDisconnectFunctionTests()
    {
        _connectionServiceMock = new Mock<IConnectionService>();
        _context = new TestLambdaContext();
        
        // テスト専用DIコンテナを作成してモックを注入
        _function = CreateFunctionWithMockedService();
    }

    private global::WebSocketDisconnectFunction.Function CreateFunctionWithMockedService()
    {
        // 通常のコンストラクタでFunctionを作成
        var function = new global::WebSocketDisconnectFunction.Function();
        
        // リフレクションで_connectionServiceフィールドをモックに置き換え
        var connectionServiceField = typeof(global::WebSocketDisconnectFunction.Function)
            .GetField("_connectionService", BindingFlags.NonPublic | BindingFlags.Instance);
        connectionServiceField?.SetValue(function, _connectionServiceMock.Object);
        
        return function;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_有効な切断リクエスト_切断成功()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-123",
                RouteKey = "$disconnect"
            }
        };

        var serviceResponse = new DisconnectServiceResponse
        {
            ConnectionId = "test-connection-123"
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.Is<DisconnectServiceRequest>(r =>
            r.ConnectionId == "test-connection-123")), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-123", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_長いConnectionId_切断成功()
    {
        // Arrange
        var longConnectionId = new string('c', 128); // 128文字の接続ID
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = longConnectionId,
                RouteKey = "$disconnect"
            }
        };

        var serviceResponse = new DisconnectServiceResponse
        {
            ConnectionId = longConnectionId
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.Is<DisconnectServiceRequest>(r =>
            r.ConnectionId == longConnectionId)), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains($"WebSocket Disconnect - ConnectionId: {longConnectionId}", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_特殊文字ConnectionId_切断成功()
    {
        // Arrange
        var specialConnectionId = "conn-123_456.789$test@example";
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = specialConnectionId,
                RouteKey = "$disconnect"
            }
        };

        var serviceResponse = new DisconnectServiceResponse
        {
            ConnectionId = specialConnectionId
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.Is<DisconnectServiceRequest>(r =>
            r.ConnectionId == specialConnectionId)), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ConnectionService例外発生でも成功レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-exception",
                RouteKey = "$disconnect"
            }
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ThrowsAsync(new InvalidOperationException("Connection not found"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        // 切断処理のエラーは成功として扱われる
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-exception", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("InvalidOperationException", logMessages);
        Assert.Contains("Connection not found", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_KeyNotFoundException_成功レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-notfound",
                RouteKey = "$disconnect"
            }
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ThrowsAsync(new KeyNotFoundException("Connection record not found"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        // 接続が見つからない場合でも成功として扱う
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-notfound", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("KeyNotFoundException", logMessages);
        Assert.Contains("Connection record not found", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_UnauthorizedAccessException_成功レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-unauthorized",
                RouteKey = "$disconnect"
            }
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        // 認証エラーも成功として扱う（切断時なので）
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-unauthorized", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("UnauthorizedAccessException", logMessages);
        Assert.Contains("Access denied", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_ArgumentException_成功レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-invalid",
                RouteKey = "$disconnect"
            }
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ThrowsAsync(new ArgumentException("Invalid connection ID format"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        // 引数エラーも成功として扱う
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-invalid", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("ArgumentException", logMessages);
        Assert.Contains("Invalid connection ID format", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_データベース接続エラー_成功レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-db-error",
                RouteKey = "$disconnect"
            }
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ThrowsAsync(new TimeoutException("Database connection timeout"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        // データベースタイムアウトも成功として扱う
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()), Times.Once);

        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-db-error", logMessages);
        Assert.Contains("Error:", logMessages);
        Assert.Contains("TimeoutException", logMessages);
        Assert.Contains("Database connection timeout", logMessages);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_Service正常処理_詳細ログ確認()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test-connection-detailed-log",
                RouteKey = "$disconnect"
            }
        };

        var serviceResponse = new DisconnectServiceResponse
        {
            ConnectionId = "test-connection-detailed-log"
        };

        _connectionServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<DisconnectServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Disconnected", response.Body);

        // サービス呼び出しの詳細な検証
        _connectionServiceMock.Verify(x => x.DisconnectAsync(It.Is<DisconnectServiceRequest>(req =>
            req.ConnectionId == "test-connection-detailed-log" &&
            !string.IsNullOrEmpty(req.ConnectionId))), Times.Once);

        // ログメッセージの詳細な確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("WebSocket Disconnect - ConnectionId: test-connection-detailed-log", logMessages);
        Assert.DoesNotContain("Error:", logMessages); // エラーログがないことを確認
    }
}