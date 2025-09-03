using Core.Domain.ValueObjects.Message;

namespace Core.Tests.Domain.ValueObjects.Message;

public class MessageContentTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なメッセージ内容で正常作成()
    {
        // Arrange
        var validContent = "これはテストメッセージです";

        // Act
        var messageContent = new MessageContent(validContent);

        // Assert
        Assert.Equal(validContent, messageContent.Value);
        Assert.Equal(validContent, messageContent.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_前後の空白を自動トリム()
    {
        // Arrange
        var contentWithSpaces = "  テストメッセージ  ";
        var expectedContent = "テストメッセージ";

        // Act
        var messageContent = new MessageContent(contentWithSpaces);

        // Assert
        Assert.Equal(expectedContent, messageContent.Value);
        Assert.Equal(expectedContent, messageContent.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からMessageContentへ変換()
    {
        // Arrange
        var validContent = "暗黙変換テスト";

        // Act
        MessageContent messageContent = validContent;

        // Assert
        Assert.Equal(validContent, messageContent.Value);
        Assert.Equal(validContent, messageContent.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_MessageContentから文字列へ変換()
    {
        // Arrange
        var messageContent = new MessageContent("文字列変換テスト");

        // Act
        string result = messageContent;

        // Assert
        Assert.Equal("文字列変換テスト", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var messageContent = new MessageContent("ToString表示テスト");

        // Act
        var result = messageContent.ToString();

        // Assert
        Assert.Equal("ToString表示テスト", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長500文字で正常作成()
    {
        // Arrange
        var maxLengthContent = new string('あ', MessageContent.MaxLength);

        // Act
        var messageContent = new MessageContent(maxLengthContent);

        // Assert
        Assert.Equal(maxLengthContent, messageContent.Value);
        Assert.Equal(MessageContent.MaxLength, messageContent.Value.Length);
    }

    [Fact]
    public void 境界値_コンストラクタ_1文字で正常作成()
    {
        // Arrange
        var singleChar = "A";

        // Act
        var messageContent = new MessageContent(singleChar);

        // Assert
        Assert.Equal(singleChar, messageContent.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageContent(null!));
        
        Assert.Equal("メッセージ内容は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageContent(""));
        
        Assert.Equal("メッセージ内容は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageContent("   "));
        
        Assert.Equal("メッセージ内容は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大長超過501文字でArgumentException()
    {
        // Arrange
        var overMaxLengthContent = new string('あ', MessageContent.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageContent(overMaxLengthContent));
        
        Assert.Equal("メッセージ内容は500文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大幅に最大長超過1000文字でArgumentException()
    {
        // Arrange
        var overMaxLengthContent = new string('B', 1000);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MessageContent(overMaxLengthContent));
        
        Assert.Equal("メッセージ内容は500文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MessageContent messageContent = null!;
            return messageContent;
        });
        
        Assert.Equal("メッセージ内容は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大長超過でArgumentException()
    {
        // Arrange
        var overMaxLengthContent = new string('C', MessageContent.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MessageContent messageContent = overMaxLengthContent;
            return messageContent;
        });
        
        Assert.Equal("メッセージ内容は500文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}