using Core.Domain.ValueObjects.Meeting;

namespace Core.Tests.Domain.ValueObjects.Meeting;

public class ParticipantPasswordTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なパスワードで正常作成()
    {
        // Arrange
        var validPassword = "guest123";

        // Act
        var participantPassword = new ParticipantPassword(validPassword);

        // Assert
        Assert.Equal(validPassword, participantPassword.Value);
        Assert.Equal(new string('*', validPassword.Length), participantPassword.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からParticipantPasswordへ変換()
    {
        // Arrange
        var validPassword = "user456";

        // Act
        ParticipantPassword participantPassword = validPassword;

        // Assert
        Assert.Equal(validPassword, participantPassword.Value);
        Assert.Equal(new string('*', validPassword.Length), participantPassword.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_ParticipantPasswordから文字列へ変換()
    {
        // Arrange
        var participantPassword = new ParticipantPassword("access789");

        // Act
        string result = participantPassword;

        // Assert
        Assert.Equal("access789", result);
    }

    [Fact]
    public void 正常系_ToString_セキュリティのためマスクされた文字列を返す()
    {
        // Arrange
        var participantPassword = new ParticipantPassword("participant");

        // Act
        var result = participantPassword.ToString();

        // Assert
        Assert.Equal("***********", result); // 11文字のマスク
        Assert.NotEqual("participant", result); // 実際のパスワードは返されない
    }

    [Fact]
    public void 境界値_コンストラクタ_最小長4文字で正常作成()
    {
        // Arrange
        var minLengthPassword = new string('A', ParticipantPassword.MinLength);

        // Act
        var participantPassword = new ParticipantPassword(minLengthPassword);

        // Assert
        Assert.Equal(minLengthPassword, participantPassword.Value);
        Assert.Equal(ParticipantPassword.MinLength, participantPassword.Value.Length);
        Assert.Equal(new string('*', ParticipantPassword.MinLength), participantPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長20文字で正常作成()
    {
        // Arrange
        var maxLengthPassword = new string('B', ParticipantPassword.MaxLength);

        // Act
        var participantPassword = new ParticipantPassword(maxLengthPassword);

        // Assert
        Assert.Equal(maxLengthPassword, participantPassword.Value);
        Assert.Equal(ParticipantPassword.MaxLength, participantPassword.Value.Length);
        Assert.Equal(new string('*', ParticipantPassword.MaxLength), participantPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_英数字記号混在で正常作成()
    {
        // Arrange
        var complexPassword = "User123!@#";

        // Act
        var participantPassword = new ParticipantPassword(complexPassword);

        // Assert
        Assert.Equal(complexPassword, participantPassword.Value);
        Assert.Equal(new string('*', complexPassword.Length), participantPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_数字のみで正常作成()
    {
        // Arrange
        var numericPassword = "9876543210";

        // Act
        var participantPassword = new ParticipantPassword(numericPassword);

        // Assert
        Assert.Equal(numericPassword, participantPassword.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_アルファベットのみで正常作成()
    {
        // Arrange
        var alphabetPassword = "abcdefghij";

        // Act
        var participantPassword = new ParticipantPassword(alphabetPassword);

        // Assert
        Assert.Equal(alphabetPassword, participantPassword.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword(null!));
        
        Assert.Equal("参加者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword(""));
        
        Assert.Equal("参加者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword("   "));
        
        Assert.Equal("参加者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最小長未満3文字でArgumentException()
    {
        // Arrange
        var shortPassword = new string('A', ParticipantPassword.MinLength - 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword(shortPassword));
        
        Assert.Equal("参加者用パスワードは4文字以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大長超過21文字でArgumentException()
    {
        // Arrange
        var longPassword = new string('B', ParticipantPassword.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword(longPassword));
        
        Assert.Equal("参加者用パスワードは20文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword("user pass"));
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_タブ文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword("user\tpass"));
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_改行文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword("user\npass"));
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_先頭空白でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword(" userpass"));
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_末尾空白でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantPassword("userpass "));
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantPassword participantPassword = null!;
            return participantPassword;
        });
        
        Assert.Equal("参加者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最小長未満でArgumentException()
    {
        // Arrange
        var shortPassword = "CD";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantPassword participantPassword = shortPassword;
            return participantPassword;
        });
        
        Assert.Equal("参加者用パスワードは4文字以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大長超過でArgumentException()
    {
        // Arrange
        var longPassword = new string('C', ParticipantPassword.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantPassword participantPassword = longPassword;
            return participantPassword;
        });
        
        Assert.Equal("参加者用パスワードは20文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空白文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantPassword participantPassword = "my user";
            return participantPassword;
        });
        
        Assert.Equal("参加者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}