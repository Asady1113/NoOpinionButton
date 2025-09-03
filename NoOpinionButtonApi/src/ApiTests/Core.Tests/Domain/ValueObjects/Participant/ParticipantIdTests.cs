using Core.Domain.ValueObjects.Participant;

namespace Core.Tests.Domain.ValueObjects.Participant;

public class ParticipantIdTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なIDで正常作成()
    {
        // Arrange
        var validId = "participant123";

        // Act
        var participantId = new ParticipantId(validId);

        // Assert
        Assert.Equal(validId, participantId.Value);
        Assert.Equal(validId, participantId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_文字列からParticipantIdへ変換()
    {
        // Arrange
        var validId = "participant456";

        // Act
        ParticipantId participantId = validId;

        // Assert
        Assert.Equal(validId, participantId.Value);
        Assert.Equal(validId, participantId.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_ParticipantIdから文字列へ変換()
    {
        // Arrange
        var participantId = new ParticipantId("participant789");

        // Act
        string result = participantId;

        // Assert
        Assert.Equal("participant789", result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var participantId = new ParticipantId("participant-abc-123");

        // Act
        var result = participantId.ToString();

        // Assert
        Assert.Equal("participant-abc-123", result);
    }

    [Fact]
    public void 異常系_コンストラクタ_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantId(null!));
        
        Assert.Equal("参加者IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空文字列でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantId(""));
        
        Assert.Equal("参加者IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_空白文字でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ParticipantId("   "));
        
        Assert.Equal("参加者IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_null値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ParticipantId participantId = null!;
            return participantId;
        });
        
        Assert.Equal("参加者IDは空にできません (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}