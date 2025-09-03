using Core.Domain.ValueObjects.Participant;

namespace Core.Tests.Domain.ValueObjects.Participant;

public class NoOpinionPointTests
{
    [Fact]
    public void 正常系_コンストラクタ_有効なポイントで正常作成()
    {
        // Arrange
        var validPoint = 1;

        // Act
        var noOpinionPoint = new NoOpinionPoint(validPoint);

        // Assert
        Assert.Equal(validPoint, noOpinionPoint.Value);
        Assert.Equal("1", noOpinionPoint.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_整数からNoOpinionPointへ変換()
    {
        // Arrange
        var validPoint = 2;

        // Act
        NoOpinionPoint noOpinionPoint = validPoint;

        // Assert
        Assert.Equal(validPoint, noOpinionPoint.Value);
        Assert.Equal("2", noOpinionPoint.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_NoOpinionPointから整数へ変換()
    {
        // Arrange
        var noOpinionPoint = new NoOpinionPoint(1);

        // Act
        int result = noOpinionPoint;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var noOpinionPoint = new NoOpinionPoint(2);

        // Act
        var result = noOpinionPoint.ToString();

        // Assert
        Assert.Equal("2", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_最小値0で正常作成()
    {
        // Act
        var noOpinionPoint = new NoOpinionPoint(0);

        // Assert
        Assert.Equal(0, noOpinionPoint.Value);
        Assert.Equal("0", noOpinionPoint.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_中間値1で正常作成()
    {
        // Act
        var noOpinionPoint = new NoOpinionPoint(1);

        // Assert
        Assert.Equal(1, noOpinionPoint.Value);
        Assert.Equal("1", noOpinionPoint.ToString());
    }

    [Fact]
    public void 境界値_コンストラクタ_最大値2で正常作成()
    {
        // Act
        var noOpinionPoint = new NoOpinionPoint(NoOpinionPoint.MaxPoint);

        // Assert
        Assert.Equal(NoOpinionPoint.MaxPoint, noOpinionPoint.Value);
        Assert.Equal("2", noOpinionPoint.ToString());
    }

    [Fact]
    public void 境界値_暗黙的変換_最小値0で正常作成()
    {
        // Act
        NoOpinionPoint noOpinionPoint = 0;

        // Assert
        Assert.Equal(0, noOpinionPoint.Value);
    }

    [Fact]
    public void 境界値_暗黙的変換_最大値2で正常作成()
    {
        // Act
        NoOpinionPoint noOpinionPoint = 2;

        // Assert
        Assert.Equal(2, noOpinionPoint.Value);
    }

    [Fact]
    public void 異常系_コンストラクタ_負の値マイナス1でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new NoOpinionPoint(-1));
        
        Assert.Equal("意見なしポイントは0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大幅な負の値マイナス10でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new NoOpinionPoint(-10));
        
        Assert.Equal("意見なしポイントは0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_最大値超過3でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new NoOpinionPoint(3));
        
        Assert.Equal("意見なしポイントは2以下である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大幅な最大値超過10でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new NoOpinionPoint(10));
        
        Assert.Equal("意見なしポイントは2以下である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_負の値でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            NoOpinionPoint noOpinionPoint = -1;
            return noOpinionPoint;
        });
        
        Assert.Equal("意見なしポイントは0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_最大値超過でArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            NoOpinionPoint noOpinionPoint = NoOpinionPoint.MaxPoint + 1;
            return noOpinionPoint;
        });
        
        Assert.Equal("意見なしポイントは2以下である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void 境界値テスト_有効範囲内のすべての値で正常作成(int validValue)
    {
        // Act
        var noOpinionPoint = new NoOpinionPoint(validValue);

        // Assert
        Assert.Equal(validValue, noOpinionPoint.Value);
        Assert.Equal(validValue.ToString(), noOpinionPoint.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(100)]
    public void 境界値テスト_無効範囲のすべての値でArgumentException(int invalidValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new NoOpinionPoint(invalidValue));
        
        // 負の値の場合と最大値超過の場合で異なるメッセージを検証
        if (invalidValue < 0)
        {
            Assert.Equal("意見なしポイントは0以上である必要があります (Parameter 'value')", exception.Message);
        }
        else
        {
            Assert.Equal("意見なしポイントは2以下である必要があります (Parameter 'value')", exception.Message);
        }
        
        Assert.Equal("value", exception.ParamName);
    }
}