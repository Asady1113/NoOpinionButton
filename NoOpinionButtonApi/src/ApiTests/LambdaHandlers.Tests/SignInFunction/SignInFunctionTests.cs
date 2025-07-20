using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application;
using Moq;
using SignInFunction.DTOs;
using Common.ApiResponse;

namespace LambdaHandlers.Tests.SignInFunction;

public class SignInFunctionTests
{
    private readonly Mock<ISignInService> _signInServiceMock;
    private readonly global::SignInFunction.Function _function;
    private readonly TestLambdaContext _context;

    public SignInFunctionTests()
    {
        _signInServiceMock = new Mock<ISignInService>();
        _context = new TestLambdaContext();
        
        // テスト専用DIコンテナを作成してモックを注入
        _function = CreateFunctionWithMockedService();
    }

    private global::SignInFunction.Function CreateFunctionWithMockedService()
    {
        // 通常のコンストラクタでFunctionを作成
        var function = new global::SignInFunction.Function();
        
        // リフレクションで_signInServiceフィールドをモックに置き換え
        var signInServiceField = typeof(global::SignInFunction.Function)
            .GetField("_signInService", BindingFlags.NonPublic | BindingFlags.Instance);
        signInServiceField?.SetValue(function, _signInServiceMock.Object);
        
        return function;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_POST_signin_有効なリクエストで正常レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { meetingId = "meeting123", password = "password123" })
        };

        var serviceResponse = new SignInServiceResponse
        {
            Id = "participant456",
            MeetingId = "meeting123",
            MeetingName = "テスト会議",
            IsFacilitator = true
        };

        _signInServiceMock
            .Setup(x => x.SignInAsync(It.IsAny<SignInServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<SignInResponse>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("participant456", apiResponse.Data.Id);
        Assert.Equal("meeting123", apiResponse.Data.MeetingId);
        Assert.Equal("テスト会議", apiResponse.Data.MeetingName);
        Assert.True(apiResponse.Data.IsFacilitator);

        _signInServiceMock.Verify(x => x.SignInAsync(It.Is<SignInServiceRequest>(r =>
            r.MeetingId == "meeting123" && r.Password == "password123")), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_OPTIONS_signin_CORS正常レスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
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
    public async Task 異常系_FunctionHandler_無効なHTTPメソッド_NotFound()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
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
    public async Task 異常系_FunctionHandler_空のリクエストボディ_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
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
            Path = "/signin",
            HttpMethod = "POST",
            Body = "{ invalid json"
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Invalid JSON format", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_必須フィールド欠如_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { meetingId = "meeting123" }) // passwordが欠如
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("'meetingId' and 'password' are required", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_認証失敗_Unauthorized()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { meetingId = "meeting123", password = "wrongpassword" })
        };

        _signInServiceMock
            .Setup(x => x.SignInAsync(It.IsAny<SignInServiceRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid password"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(401, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Invalid password", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_会議が見つからない_NotFound()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            Path = "/signin",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { meetingId = "nonexistent", password = "password123" })
        };

        _signInServiceMock
            .Setup(x => x.SignInAsync(It.IsAny<SignInServiceRequest>()))
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
            Path = "/signin",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { meetingId = "meeting123", password = "password123" })
        };

        _signInServiceMock
            .Setup(x => x.SignInAsync(It.IsAny<SignInServiceRequest>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

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
        Assert.Contains("Unexpected error", logMessages);
    }
}