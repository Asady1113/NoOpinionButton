using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Infrastructure.Repository;
using Moq;
using System.Text;

namespace Infrastructure.Tests.Repositories;

public class BroadcastRepositoryTests : IDisposable
{
    private readonly Mock<AmazonApiGatewayManagementApiClient> _managementApiClientMock;
    private readonly BroadcastRepository _repository;
    private readonly string _originalWebSocketEndpoint;

    public BroadcastRepositoryTests()
    {
        // 環境変数の元の値を保存
        _originalWebSocketEndpoint = Environment.GetEnvironmentVariable("WEBSOCKET_API_ENDPOINT") ?? string.Empty;
        
        // テスト用環境変数を設定
        Environment.SetEnvironmentVariable("WEBSOCKET_API_ENDPOINT", "wss://test-api.execute-api.us-east-1.amazonaws.com/prod");
        
        // AmazonApiGatewayManagementApiClientのモックを作成
        _managementApiClientMock = new Mock<AmazonApiGatewayManagementApiClient>();
        
        // BroadcastRepositoryのインスタンスを作成
        _repository = new BroadcastRepository();
        
        // プライベートフィールドの_managementApiClientをモックに置き換え
        var field = typeof(BroadcastRepository).GetField("_managementApiClient", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(_repository, _managementApiClientMock.Object);
    }

    [Fact]
    public async Task 正常系_BroadcastToConnectionAsync_単一接続への配信成功()
    {
        // Arrange
        var connectionId = "connection123";
        var message = "Test message for single connection broadcast";
        var expectedBytes = Encoding.UTF8.GetBytes(message);

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        // Act
        var result = await _repository.BroadcastToConnectionAsync(connectionId, message);

        // Assert
        Assert.True(result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.Is<PostToConnectionRequest>(req => 
                req.ConnectionId == connectionId && 
                req.Data != null &&
                req.Data.Length == expectedBytes.Length), 
            default), Times.Once);
    }

    [Fact]
    public async Task 正常系_BroadcastToMultipleConnectionsAsync_複数接続への並列配信成功()
    {
        // Arrange
        var connectionIds = new[] { "connection1", "connection2", "connection3" };
        var message = "Broadcast message to multiple connections for parallel processing test";

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        // Act
        var result = await _repository.BroadcastToMultipleConnectionsAsync(connectionIds, message);

        // Assert
        Assert.Equal(3, result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.IsAny<PostToConnectionRequest>(), default), Times.Exactly(3));
        
        // 各接続IDに対して正しい呼び出しが行われたか確認
        foreach (var connectionId in connectionIds)
        {
            _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
                It.Is<PostToConnectionRequest>(req => req.ConnectionId == connectionId), 
                default), Times.Once);
        }
    }

    [Fact]
    public async Task 正常系_BroadcastToMultipleConnectionsAsync_空の接続リストで0を返す()
    {
        // Arrange
        var connectionIds = new string[0];
        var message = "Message for empty connection list test";

        // Act
        var result = await _repository.BroadcastToMultipleConnectionsAsync(connectionIds, message);

        // Assert
        Assert.Equal(0, result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.IsAny<PostToConnectionRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task 異常系_BroadcastToConnectionAsync_GoneException処理でfalseを返す()
    {
        // Arrange
        var connectionId = "disconnected-connection";
        var message = "Message to disconnected connection causing GoneException";
        var goneException = new GoneException("Connection is no longer available");

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .ThrowsAsync(goneException);

        // Act
        var result = await _repository.BroadcastToConnectionAsync(connectionId, message);

        // Assert
        Assert.False(result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.Is<PostToConnectionRequest>(req => req.ConnectionId == connectionId), 
            default), Times.Once);
    }

    [Fact]
    public async Task 異常系_BroadcastToConnectionAsync_一般例外処理でfalseを返す()
    {
        // Arrange
        var connectionId = "error-connection";
        var message = "Message causing general exception during broadcast operation";
        var generalException = new InvalidOperationException("General API Gateway error");

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .ThrowsAsync(generalException);

        // Act
        var result = await _repository.BroadcastToConnectionAsync(connectionId, message);

        // Assert
        Assert.False(result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.Is<PostToConnectionRequest>(req => req.ConnectionId == connectionId), 
            default), Times.Once);
    }

    [Fact]
    public void 異常系_Constructor_環境変数未設定でInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("WEBSOCKET_API_ENDPOINT", null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new BroadcastRepository());
        Assert.Equal("WEBSOCKET_API_ENDPOINT environment variable is not set", exception.Message);
    }

    [Fact]
    public async Task 境界値_BroadcastToConnectionAsync_500文字メッセージの配信成功()
    {
        // Arrange
        var connectionId = "boundary-test-connection";
        // MessageContent制限に合わせて500文字以内のテストメッセージ
        var message = new string('A', 500);
        var expectedBytes = Encoding.UTF8.GetBytes(message);

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        // Act
        var result = await _repository.BroadcastToConnectionAsync(connectionId, message);

        // Assert
        Assert.True(result);
        Assert.Equal(500, message.Length);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.Is<PostToConnectionRequest>(req => 
                req.ConnectionId == connectionId && 
                req.Data != null &&
                req.Data.Length == expectedBytes.Length), 
            default), Times.Once);
    }

    [Fact]
    public async Task 境界値_BroadcastToMultipleConnectionsAsync_一部接続失敗時の成功数カウント()
    {
        // Arrange
        var connectionIds = new[] { "success1", "fail1", "success2", "fail2", "success3" };
        var message = "Mixed success and failure test message for partial broadcast completion";

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(
                It.Is<PostToConnectionRequest>(req => req.ConnectionId.StartsWith("success")), 
                default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(
                It.Is<PostToConnectionRequest>(req => req.ConnectionId.StartsWith("fail")), 
                default))
            .ThrowsAsync(new GoneException("Connection failed"));

        // Act
        var result = await _repository.BroadcastToMultipleConnectionsAsync(connectionIds, message);

        // Assert
        Assert.Equal(3, result); // success1, success2, success3が成功
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.IsAny<PostToConnectionRequest>(), default), Times.Exactly(5));
    }

    [Fact]
    public async Task 特殊ケース_BroadcastToMultipleConnectionsAsync_重複接続IDの処理()
    {
        // Arrange
        var connectionIds = new[] { "duplicate", "unique", "duplicate", "unique" };
        var message = "Duplicate connection ID handling test for broadcast repository";

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        // Act
        var result = await _repository.BroadcastToMultipleConnectionsAsync(connectionIds, message);

        // Assert
        // 重複があってもすべて配信される（リポジトリ層では重複排除しない）
        Assert.Equal(4, result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.IsAny<PostToConnectionRequest>(), default), Times.Exactly(4));
    }

    [Fact]
    public async Task 特殊ケース_BroadcastToConnectionAsync_UTF8エンコーディング正常動作()
    {
        // Arrange
        var connectionId = "utf8-test-connection";
        var message = "日本語メッセージテスト：UTF-8エンコーディング確認用テキスト";
        var expectedBytes = Encoding.UTF8.GetBytes(message);

        _managementApiClientMock
            .Setup(x => x.PostToConnectionAsync(It.IsAny<PostToConnectionRequest>(), default))
            .Returns(Task.FromResult(new PostToConnectionResponse()));

        // Act
        var result = await _repository.BroadcastToConnectionAsync(connectionId, message);

        // Assert
        Assert.True(result);
        _managementApiClientMock.Verify(x => x.PostToConnectionAsync(
            It.Is<PostToConnectionRequest>(req => 
                req.ConnectionId == connectionId && 
                req.Data != null &&
                req.Data.Length == expectedBytes.Length), 
            default), Times.Once);
    }

    public void Dispose()
    {
        // 環境変数を元に戻す
        Environment.SetEnvironmentVariable("WEBSOCKET_API_ENDPOINT", _originalWebSocketEndpoint);
        
        // リポジトリのリソースを解放
        _repository?.Dispose();
        
        // モックのクリーンアップ
        _managementApiClientMock?.Reset();
    }
}