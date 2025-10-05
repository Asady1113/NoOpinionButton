using Core.Domain.ValueObjects.Meeting;

namespace Core.Tests.Domain.ValueObjects.Meeting;

public class MeetingNameTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なミーティング名で正常作成()
    {
        // Arrange
        var validName = "週次定例会議";

        // Act
        var meetingName = new MeetingName(validName);

        // Assert
        Assert.Equal(validName, meetingName.Value);
        Assert.Equal(validName, meetingName.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_前後の空白を自動トリム()
    {
        // Arrange
        var nameWithSpaces = "  月次会議  ";
        var expectedName = "月次会議";

        // Act
        var meetingName = new MeetingName(nameWithSpaces);

        // Assert
        Assert.Equal(expectedName, meetingName.Value);
        Assert.Equal(expectedName, meetingName.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からMeetingNameへ変換()
    {
        // Arrange
        var validName = "プロジェクト会議";

        // Act
        MeetingName meetingName = validName;

        // Assert
        Assert.Equal(validName, meetingName.Value);
        Assert.Equal(validName, meetingName.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_MeetingNameから文字列へ変換()
    {
        // Arrange
        var meetingName = new MeetingName("チーム会議");

        // Act
        string result = meetingName;

        // Assert
        Assert.Equal("チーム会議", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var meetingName = new MeetingName("システム設計会議");

        // Act
        var result = meetingName.ToString();

        // Assert
        Assert.Equal("システム設計会議", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長100文字で正常作成()
    {
        // Arrange
        var maxLengthName = new string('あ', MeetingName.MaxLength);

        // Act
        var meetingName = new MeetingName(maxLengthName);

        // Assert
        Assert.Equal(maxLengthName, meetingName.Value);
        Assert.Equal(MeetingName.MaxLength, meetingName.Value.Length);
    }

    [Fact]
    public void 境界値_コンストラクタ_1文字で正常作成()
    {
        // Arrange
        var singleChar = "A";

        // Act
        var meetingName = new MeetingName(singleChar);

        // Assert
        Assert.Equal(singleChar, meetingName.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_最大長100文字前後の空白トリムで正常作成()
    {
        // Arrange - 空白を含めて100文字、トリム後98文字になる
        var nameWithSpaces = "  " + new string('B', MeetingName.MaxLength - 4) + "  ";
        var expectedName = new string('B', MeetingName.MaxLength - 4);

        // Act
        var meetingName = new MeetingName(nameWithSpaces);

        // Assert
        Assert.Equal(expectedName, meetingName.Value);
        Assert.Equal(MeetingName.MaxLength - 4, meetingName.Value.Length);
    }

    [Fact]
    public void 境界値_コンストラクタ_英数字混在で正常作成()
    {
        // Arrange
        var mixedName = "Meeting ABC 123";

        // Act
        var meetingName = new MeetingName(mixedName);

        // Assert
        Assert.Equal(mixedName, meetingName.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_特殊文字含む名前で正常作成()
    {
        // Arrange
        var specialName = "会議-2024/01/15 (重要)";

        // Act
        var meetingName = new MeetingName(specialName);

        // Assert
        Assert.Equal(specialName, meetingName.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName(null!));
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName(""));
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName("   "));
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大長超過101文字でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('あ', MeetingName.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName(overMaxLengthName));
        
        Assert.Equal("ミーティング名は100文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大幅に最大長超過200文字でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('B', 200);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName(overMaxLengthName));
        
        Assert.Equal("ミーティング名は100文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_タブ文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName("\t"));
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_改行文字のみでArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingName("\n"));
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MeetingName meetingName = null!;
            return meetingName;
        });
        
        Assert.Equal("ミーティング名は空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大長超過でArgumentException()
    {
        // Arrange
        var overMaxLengthName = new string('C', MeetingName.MaxLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MeetingName meetingName = overMaxLengthName;
            return meetingName;
        });
        
        Assert.Equal("ミーティング名は100文字以内である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}