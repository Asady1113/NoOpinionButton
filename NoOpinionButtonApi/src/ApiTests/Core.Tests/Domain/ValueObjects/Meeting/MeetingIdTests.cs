using Core.Domain.ValueObjects.Meeting;

namespace Core.Tests.Domain.ValueObjects.Meeting;

public class MeetingIdTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なIDで正常作成()
    {
        // Arrange
        var validId = "meeting123";

        // Act
        var meetingId = new MeetingId(validId);

        // Assert
        Assert.Equal(validId, meetingId.Value);
        Assert.Equal(validId, meetingId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からMeetingIdへ変換()
    {
        // Arrange
        var validId = "meeting456";

        // Act
        MeetingId meetingId = validId;

        // Assert
        Assert.Equal(validId, meetingId.Value);
        Assert.Equal(validId, meetingId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_MeetingIdから文字列へ変換()
    {
        // Arrange
        var meetingId = new MeetingId("meeting789");

        // Act
        string result = meetingId;

        // Assert
        Assert.Equal("meeting789", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var meetingId = new MeetingId("meeting-abc-123");

        // Act
        var result = meetingId.ToString();

        // Assert
        Assert.Equal("meeting-abc-123", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_1文字で正常作成()
    {
        // Arrange
        var singleChar = "A";

        // Act
        var meetingId = new MeetingId(singleChar);

        // Assert
        Assert.Equal(singleChar, meetingId.Value);
    }

    [Fact]
    public void 境界値_コンストラクタ_特殊文字含む識別子で正常作成()
    {
        // Arrange
        var specialId = "meeting_123-456.789";

        // Act
        var meetingId = new MeetingId(specialId);

        // Assert
        Assert.Equal(specialId, meetingId.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingId(null!));
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingId(""));
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingId("   "));
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_タブ文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingId("\t"));
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_改行文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MeetingId("\n"));
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MeetingId meetingId = null!;
            return meetingId;
        });
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MeetingId meetingId = "";
            return meetingId;
        });
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            MeetingId meetingId = "   ";
            return meetingId;
        });
        
        Assert.Equal("会議IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}