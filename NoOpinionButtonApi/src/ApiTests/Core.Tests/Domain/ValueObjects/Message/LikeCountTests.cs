using Core.Domain.ValueObjects.Message;

namespace Core.Tests.Domain.ValueObjects.Message;

public class LikeCountTests
{
    [Fact]
    public void 正常系_コンストラクタ_0でデフォルト値正常作成()
    {
        // Act
        var likeCount = new LikeCount();

        // Assert
        Assert.Equal(0, likeCount.Value);
        Assert.Equal("0", likeCount.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_正の値で正常作成()
    {
        // Arrange
        var validCount = 10;

        // Act
        var likeCount = new LikeCount(validCount);

        // Assert
        Assert.Equal(validCount, likeCount.Value);
        Assert.Equal("10", likeCount.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_0で正常作成()
    {
        // Arrange
        var zeroCount = 0;

        // Act
        var likeCount = new LikeCount(zeroCount);

        // Assert
        Assert.Equal(zeroCount, likeCount.Value);
        Assert.Equal("0", likeCount.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_整数からLikeCountへ変換()
    {
        // Arrange
        var validCount = 25;

        // Act
        LikeCount likeCount = validCount;

        // Assert
        Assert.Equal(validCount, likeCount.Value);
        Assert.Equal("25", likeCount.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_LikeCountから整数へ変換()
    {
        // Arrange
        var likeCount = new LikeCount(42);

        // Act
        int result = likeCount;

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var likeCount = new LikeCount(99);

        // Act
        var result = likeCount.ToString();

        // Assert
        Assert.Equal("99", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_int最大値で正常作成()
    {
        // Arrange
        var maxValue = int.MaxValue;

        // Act
        var likeCount = new LikeCount(maxValue);

        // Assert
        Assert.Equal(maxValue, likeCount.Value);
        Assert.Equal(maxValue.ToString(), likeCount.ToString());
    }

    [Fact]
    public void 異常系_コンストラクタ_負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LikeCount(negativeValue));
        
        Assert.Equal("いいね数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大きな負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -100;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LikeCount(negativeValue));
        
        Assert.Equal("いいね数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_int最小値でArgumentException()
    {
        // Arrange
        var minValue = int.MinValue;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LikeCount(minValue));
        
        Assert.Equal("いいね数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -5;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            LikeCount likeCount = negativeValue;
            return likeCount;
        });
        
        Assert.Equal("いいね数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}