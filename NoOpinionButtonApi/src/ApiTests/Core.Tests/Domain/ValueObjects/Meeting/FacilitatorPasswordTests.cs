using Core.Domain.ValueObjects.Meeting;

namespace Core.Tests.Domain.ValueObjects.Meeting;

public class FacilitatorPasswordTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なパスワードで正常作成()
    {
        // Arrange
        var validPassword = "secure123";

        // Act
        var facilitatorPassword = new FacilitatorPassword(validPassword);

        // Assert
        Assert.Equal(validPassword, facilitatorPassword.Value);
        Assert.Equal(new string('*', validPassword.Length), facilitatorPassword.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からFacilitatorPasswordへ変換()
    {
        // Arrange
        var validPassword = "password456";

        // Act
        FacilitatorPassword facilitatorPassword = validPassword;

        // Assert
        Assert.Equal(validPassword, facilitatorPassword.Value);
        Assert.Equal(new string('*', validPassword.Length), facilitatorPassword.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_FacilitatorPasswordから文字列へ変換()
    {
        // Arrange
        var facilitatorPassword = new FacilitatorPassword("mypassword");

        // Act
        string result = facilitatorPassword;

        // Assert
        Assert.Equal("mypassword", result);
    }

    [Fact]
    public void 正常系_ToString_セキュリティのためマスクされた文字列を返す()
    {
        // Arrange
        var facilitatorPassword = new FacilitatorPassword("secretpass");

        // Act
        var result = facilitatorPassword.ToString();

        // Assert
        Assert.Equal("**********", result); // 10文字のマスク
        Assert.NotEqual("secretpass", result); // 実際のパスワードは返されない
    }

    [Fact]
    public void 境界値_コンストラクタ_最小長4文字で正常作成()
    {
        // Arrange
        var minLengthPassword = new string('A', FacilitatorPassword.MinLength);

        // Act
        var facilitatorPassword = new FacilitatorPassword(minLengthPassword);

        // Assert
        Assert.Equal(minLengthPassword, facilitatorPassword.Value);
        Assert.Equal(FacilitatorPassword.MinLength, facilitatorPassword.Value.Length);
        Assert.Equal(new string('*', FacilitatorPassword.MinLength), facilitatorPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長20文字で正常作成()
    {
        // Arrange
        var maxLengthPassword = new string('B', FacilitatorPassword.MaxLength);

        // Act
        var facilitatorPassword = new FacilitatorPassword(maxLengthPassword);

        // Assert
        Assert.Equal(maxLengthPassword, facilitatorPassword.Value);
        Assert.Equal(FacilitatorPassword.MaxLength, facilitatorPassword.Value.Length);
        Assert.Equal(new string('*', FacilitatorPassword.MaxLength), facilitatorPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_英数字記号混在で正常作成()
    {
        // Arrange
        var complexPassword = "Abc123!@#";

        // Act
        var facilitatorPassword = new FacilitatorPassword(complexPassword);

        // Assert
        Assert.Equal(complexPassword, facilitatorPassword.Value);
        Assert.Equal(new string('*', complexPassword.Length), facilitatorPassword.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_数字のみで正常作成()
    {
        // Arrange
        var numericPassword = "1234567890";

        // Act
        var facilitatorPassword = new FacilitatorPassword(numericPassword);

        // Assert
        Assert.Equal(numericPassword, facilitatorPassword.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_アルファベットのみで正常作成()
    {
        // Arrange
        var alphabetPassword = "ABCDEFGHIJ";

        // Act
        var facilitatorPassword = new FacilitatorPassword(alphabetPassword);

        // Assert
        Assert.Equal(alphabetPassword, facilitatorPassword.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword(null!));
        
        Assert.Equal("司会者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword(""));
        
        Assert.Equal("司会者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword("   "));
        
        Assert.Equal("司会者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最小長未満3文字でArgumentException()
    {
        // Arrange
        var shortPassword = new string('A', FacilitatorPassword.MinLength - 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword(shortPassword));
        
        Assert.Equal("司会者用パスワードは4文字以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大長超過21文字でArgumentException()
    {
        // Arrange
        var longPassword = new string('B', FacilitatorPassword.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword(longPassword));
        
        Assert.Equal("司会者用パスワードは20文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword("pass word"));
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_タブ文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword("pass\tword"));
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_改行文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword("pass\nword"));
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_先頭空白でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword(" password"));
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_末尾空白でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FacilitatorPassword("password "));
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            FacilitatorPassword facilitatorPassword = null!;
            return facilitatorPassword;
        });
        
        Assert.Equal("司会者用パスワードは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最小長未満でArgumentException()
    {
        // Arrange
        var shortPassword = "AB";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            FacilitatorPassword facilitatorPassword = shortPassword;
            return facilitatorPassword;
        });
        
        Assert.Equal("司会者用パスワードは4文字以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大長超過でArgumentException()
    {
        // Arrange
        var longPassword = new string('C', FacilitatorPassword.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            FacilitatorPassword facilitatorPassword = longPassword;
            return facilitatorPassword;
        });
        
        Assert.Equal("司会者用パスワードは20文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空白文字含むでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            FacilitatorPassword facilitatorPassword = "my password";
            return facilitatorPassword;
        });
        
        Assert.Equal("司会者用パスワードに空白文字を含めることはできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}