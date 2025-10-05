using Core.Domain.ValueObjects.Participant;

namespace Core.Tests.Domain.ValueObjects.Participant;

public class ParticipantNameTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効な参加者名で正常作成()
    {
        // Arrange
        var validName = "田中太郎";

        // Act
        var participantName = new ParticipantName(validName);

        // Assert
        Assert.Equal(validName, participantName.Value);
        Assert.Equal(validName, participantName.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_前後の空白を自動トリム()
    {
        // Arrange
        var nameWithSpaces = "  山田花子  ";
        var expectedName = "山田花子";

        // Act
        var participantName = new ParticipantName(nameWithSpaces);

        // Assert
        Assert.Equal(expectedName, participantName.Value);
        Assert.Equal(expectedName, participantName.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からParticipantNameへ変換()
    {
        // Arrange
        var validName = "鈴木次郎";

        // Act
        ParticipantName participantName = validName;

        // Assert
        Assert.Equal(validName, participantName.Value);
        Assert.Equal(validName, participantName.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_ParticipantNameから文字列へ変換()
    {
        // Arrange
        var participantName = new ParticipantName("佐藤三郎");

        // Act
        string result = participantName;

        // Assert
        Assert.Equal("佐藤三郎", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var participantName = new ParticipantName("高橋四郎");

        // Act
        var result = participantName.ToString();

        // Assert
        Assert.Equal("高橋四郎", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長50文字で正常作成()
    {
        // Arrange
        var maxLengthName = new string('あ', ParticipantName.MaxLength);

        // Act
        var participantName = new ParticipantName(maxLengthName);

        // Assert
        Assert.Equal(maxLengthName, participantName.Value);
        Assert.Equal(ParticipantName.MaxLength, participantName.Value.Length);
    }

    [Fact]
    public void 境界値_コンストラクタ_1文字で正常作成()
    {
        // Arrange
        var singleChar = "A";

        // Act
        var participantName = new ParticipantName(singleChar);

        // Assert
        Assert.Equal(singleChar, participantName.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長50文字前後の空白トリムで正常作成()
    {
        // Arrange - 空白を含めて50文字、トリム後48文字になる
        var nameWithSpaces = "  " + new string('B', ParticipantName.MaxLength - 4) + "  ";
        var expectedName = new string('B', ParticipantName.MaxLength - 4);

        // Act
        var participantName = new ParticipantName(nameWithSpaces);

        // Assert
        Assert.Equal(expectedName, participantName.Value);
        Assert.Equal(ParticipantName.MaxLength - 4, participantName.Value.Length);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantName(null!));
        
        Assert.Equal("参加者名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantName(""));
        
        Assert.Equal("参加者名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantName("   "));
        
        Assert.Equal("参加者名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大長超過51文字でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('あ', ParticipantName.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantName(overMaxLengthName));
        
        Assert.Equal("参加者名は50文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大幅に最大長超過100文字でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('B', 100);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantName(overMaxLengthName));
        
        Assert.Equal("参加者名は50文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantName participantName = null!;
            return participantName;
        });
        
        Assert.Equal("参加者名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大長超過でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('C', ParticipantName.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantName participantName = overMaxLengthName;
            return participantName;
        });
        
        Assert.Equal("参加者名は50文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}