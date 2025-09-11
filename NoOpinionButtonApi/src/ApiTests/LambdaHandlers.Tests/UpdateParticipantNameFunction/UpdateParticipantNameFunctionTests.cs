using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Core.Application.Ports;
using Core.Application;
using Moq;
using UpdateParticipantNameFunction.DTOs;
using Common.ApiResponse;

namespace LambdaHandlers.Tests.UpdateParticipantNameFunction;

public class UpdateParticipantNameFunctionTests
{
    private readonly Mock<IParticipantUpdateService> _participantUpdateServiceMock;
    private readonly global::UpdateParticipantNameFunction.Function _function;
    private readonly TestLambdaContext _context;

    public UpdateParticipantNameFunctionTests()
    {
        _participantUpdateServiceMock = new Mock<IParticipantUpdateService>();
        _context = new TestLambdaContext();
        
        _function = CreateFunctionWithMockedService();
    }

    private global::UpdateParticipantNameFunction.Function CreateFunctionWithMockedService()
    {
        var function = new global::UpdateParticipantNameFunction.Function();
        
        var participantUpdateServiceField = typeof(global::UpdateParticipantNameFunction.Function)
            .GetField("_participantUpdateService", BindingFlags.NonPublic | BindingFlags.Instance);
        participantUpdateServiceField?.SetValue(function, _participantUpdateServiceMock.Object);
        
        return function;
    }

    [Fact]
    public async Task 正常系_FunctionHandler_PUT_有効なリクエストで正常レスポンス()
    {
        // Arrange
        var updateParticipantNameRequest = new UpdateParticipantNameRequest
        {
            Name = "新しい名前"
        };

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
            Body = JsonSerializer.Serialize(updateParticipantNameRequest)
        };

        var serviceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name = "新しい名前"
        };

        var serviceResponse = new ParticipantUpdateServiceResponse
        {
            UpdatedName = "新しい名前"
        };

        _participantUpdateServiceMock
            .Setup(x => x.UpdateParticipantNameAsync(It.IsAny<ParticipantUpdateServiceRequest>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(200, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<UpdateParticipantNameResponse>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("新しい名前", apiResponse.Data.UpdatedName);
        
        _participantUpdateServiceMock.Verify(x => x.UpdateParticipantNameAsync(It.IsAny<ParticipantUpdateServiceRequest>()), Times.Once);
    }

    [Fact]
    public async Task 正常系_FunctionHandler_OPTIONS_CORSレスポンス()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
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
    public async Task 異常系_FunctionHandler_パスパラメータなし_BadRequest()
    {
        // Arrange
        var updateParticipantNameRequest = new UpdateParticipantNameRequest
        {
            Name = "新しい名前"
        };

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = null,
            Body = JsonSerializer.Serialize(updateParticipantNameRequest)
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("participantId", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_空のリクエストボディ_BadRequest()
    {
        // Arrange
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
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
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
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
        var updateParticipantNameRequest = new UpdateParticipantNameRequest {};

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
            Body = JsonSerializer.Serialize(updateParticipantNameRequest) // newNameが欠如
        };

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("name", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_参加者が見つからない_NotFound()
    {
        // Arrange
        var updateParticipantNameRequest = new UpdateParticipantNameRequest
        {
            Name = "新しい名前"
        };

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/nonexistent123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "nonexistent123" } },
            Body = JsonSerializer.Serialize(updateParticipantNameRequest)
        };

        _participantUpdateServiceMock
            .Setup(x => x.UpdateParticipantNameAsync(It.IsAny<ParticipantUpdateServiceRequest>()))
            .ThrowsAsync(new KeyNotFoundException("参加者が見つかりません"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(404, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("参加者が見つかりません", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_非アクティブ参加者_BadRequest()
    {
        // Arrange
        var updateParticipantNameRequest = new UpdateParticipantNameRequest
        {
            Name = "新しい名前"
        };
        
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
            Body = JsonSerializer.Serialize(updateParticipantNameRequest)
        };

        _participantUpdateServiceMock
            .Setup(x => x.UpdateParticipantNameAsync(It.IsAny<ParticipantUpdateServiceRequest>()))
            .ThrowsAsync(new InvalidOperationException("非アクティブな参加者の名前は変更できません"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(400, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("非アクティブ", apiResponse.Error);
    }

    [Fact]
    public async Task 異常系_FunctionHandler_予期しない例外_ServerError()
    {
        // Arrange
        var updateParticipantNameRequest = new UpdateParticipantNameRequest
        {
            Name = "新しい名前"
        };

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "PUT",
            Path = "/participants/participant123/name",
            PathParameters = new Dictionary<string, string> { { "participantId", "participant123" } },
            Body = JsonSerializer.Serialize(updateParticipantNameRequest)
        };

        _participantUpdateServiceMock
            .Setup(x => x.UpdateParticipantNameAsync(It.IsAny<ParticipantUpdateServiceRequest>()))
            .ThrowsAsync(new Exception("予期しないエラー"));

        // Act
        var response = await _function.FunctionHandler(request, _context);

        // Assert
        Assert.Equal(500, response.StatusCode);
        
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(response.Body);
        Assert.NotNull(apiResponse);
        Assert.Contains("Internal server error", apiResponse.Error);
        
        // ログ出力の確認
        var testLogger = (TestLambdaLogger)_context.Logger;
        var logMessages = testLogger.Buffer.ToString();
        Assert.Contains("Exception", logMessages);
        Assert.Contains("予期しないエラー", logMessages);
    }
}