using Core.Domain.ValueObjects.Message;

namespace Core.Tests.Domain.ValueObjects.Message;

public class MessageIdTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なIDで正常作成()
    {
        // Arrange
        var validId = "message123";

        // Act
        var messageId = new MessageId(validId);

        // Assert
        Assert.Equal(validId, messageId.Value);
        Assert.Equal(validId, messageId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からMessageIdへ変換()
    {
        // Arrange
        var validId = "message456";

        // Act
        MessageId messageId = validId;

        // Assert
        Assert.Equal(validId, messageId.Value);
        Assert.Equal(validId, messageId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_MessageIdから文字列へ変換()
    {
        // Arrange
        var messageId = new MessageId("message789");

        // Act
        string result = messageId;

        // Assert
        Assert.Equal("message789", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var messageId = new MessageId("message-abc-123");

        // Act
        var result = messageId.ToString();

        // Assert
        Assert.Equal("message-abc-123", result);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageId(null!));
        
        Assert.Equal("メッセージIDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageId(""));
        
        Assert.Equal("メッセージIDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageId("   "));
        
        Assert.Equal("メッセージIDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MessageId messageId = null!;
            return messageId;
        });
        
        Assert.Equal("メッセージIDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}