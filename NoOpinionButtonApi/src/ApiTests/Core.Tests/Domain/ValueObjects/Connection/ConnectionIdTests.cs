using Core.Domain.ValueObjects.Connection;

namespace Core.Tests.Domain.ValueObjects.Connection;

public class ConnectionIdTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なIDで正常作成()
    {
        // Arrange
        var validId = "connection123";

        // Act
        var connectionId = new ConnectionId(validId);

        // Assert
        Assert.Equal(validId, connectionId.Value);
        Assert.Equal(validId, connectionId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からConnectionIdへ変換()
    {
        // Arrange
        var validId = "conn456";

        // Act
        ConnectionId connectionId = validId;

        // Assert
        Assert.Equal(validId, connectionId.Value);
        Assert.Equal(validId, connectionId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_ConnectionIdから文字列へ変換()
    {
        // Arrange
        var connectionId = new ConnectionId("conn789");

        // Act
        string result = connectionId;

        // Assert
        Assert.Equal("conn789", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var connectionId = new ConnectionId("websocket-abc-123");

        // Act
        var result = connectionId.ToString();

        // Assert
        Assert.Equal("websocket-abc-123", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_1文字で正常作成()
    {
        // Arrange
        var singleChar = "A";

        // Act
        var connectionId = new ConnectionId(singleChar);

        // Assert
        Assert.Equal(singleChar, connectionId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_AWS接続ID形式で正常作成()
    {
        // Arrange - AWS API Gateway WebSocket connection ID format
        var awsConnectionId = "dGVzdC1jb25uZWN0aW9u";

        // Act
        var connectionId = new ConnectionId(awsConnectionId);

        // Assert
        Assert.Equal(awsConnectionId, connectionId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_特殊文字含む識別子で正常作成()
    {
        // Arrange
        var specialId = "conn_123-456.789";

        // Act
        var connectionId = new ConnectionId(specialId);

        // Assert
        Assert.Equal(specialId, connectionId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_長い識別子で正常作成()
    {
        // Arrange - Very long connection ID
        var longId = "very-long-connection-id-with-many-characters-and-hyphens-12345678901234567890";

        // Act
        var connectionId = new ConnectionId(longId);

        // Assert
        Assert.Equal(longId, connectionId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_英数字のみで正常作成()
    {
        // Arrange
        var alphanumericId = "Connection123ABC";

        // Act
        var connectionId = new ConnectionId(alphanumericId);

        // Assert
        Assert.Equal(alphanumericId, connectionId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_数字のみで正常作成()
    {
        // Arrange
        var numericId = "1234567890";

        // Act
        var connectionId = new ConnectionId(numericId);

        // Assert
        Assert.Equal(numericId, connectionId.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId(null!));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId(""));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId("   "));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_タブ文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId("\t"));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_改行文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId("\n"));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_全角スペースでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConnectionId("　"));
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ConnectionId connectionId = null!;
            return connectionId;
        });
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ConnectionId connectionId = "";
            return connectionId;
        });
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ConnectionId connectionId = "   ";
            return connectionId;
        });
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_タブと改行混在でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ConnectionId connectionId = "\t\n";
            return connectionId;
        });
        
        Assert.Equal("接続IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}