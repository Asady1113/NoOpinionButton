using Core.Domain.ValueObjects.Message;

namespace Core.Tests.Domain.ValueObjects.Message;

public class ReportedCountTests
{
    [Fact]
    public void 正常系_コンストラクタ_0でデフォルト値正常作成()
    {
        // Act
        var reportedCount = new ReportedCount();

        // Assert
        Assert.Equal(0, reportedCount.Value);
        Assert.Equal("0", reportedCount.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_正の値で正常作成()
    {
        // Arrange
        var validCount = 5;

        // Act
        var reportedCount = new ReportedCount(validCount);

        // Assert
        Assert.Equal(validCount, reportedCount.Value);
        Assert.Equal("5", reportedCount.ToString());
    }

    [Fact]
    public void 正常系_コンストラクタ_0で正常作成()
    {
        // Arrange
        var zeroCount = 0;

        // Act
        var reportedCount = new ReportedCount(zeroCount);

        // Assert
        Assert.Equal(zeroCount, reportedCount.Value);
        Assert.Equal("0", reportedCount.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_整数からReportedCountへ変換()
    {
        // Arrange
        var validCount = 3;

        // Act
        ReportedCount reportedCount = validCount;

        // Assert
        Assert.Equal(validCount, reportedCount.Value);
        Assert.Equal("3", reportedCount.ToString());
    }

    [Fact]
    public void 正常系_暗黙的変換_ReportedCountから整数へ変換()
    {
        // Arrange
        var reportedCount = new ReportedCount(7);

        // Act
        int result = reportedCount;

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void 正常系_ToString_正常な文字列表現を返す()
    {
        // Arrange
        var reportedCount = new ReportedCount(12);

        // Act
        var result = reportedCount.ToString();

        // Assert
        Assert.Equal("12", result);
    }

    [Fact]
    public void 境界値_コンストラクタ_int最大値で正常作成()
    {
        // Arrange
        var maxValue = int.MaxValue;

        // Act
        var reportedCount = new ReportedCount(maxValue);

        // Assert
        Assert.Equal(maxValue, reportedCount.Value);
        Assert.Equal(maxValue.ToString(), reportedCount.ToString());
    }

    [Fact]
    public void 異常系_コンストラクタ_負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ReportedCount(negativeValue));
        
        Assert.Equal("通報回数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_大きな負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -50;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ReportedCount(negativeValue));
        
        Assert.Equal("通報回数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_コンストラクタ_int最小値でArgumentException()
    {
        // Arrange
        var minValue = int.MinValue;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ReportedCount(minValue));
        
        Assert.Equal("通報回数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void 異常系_暗黙的変換_負の値でArgumentException()
    {
        // Arrange
        var negativeValue = -10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => {
            ReportedCount reportedCount = negativeValue;
            return reportedCount;
        });
        
        Assert.Equal("通報回数は0以上である必要があります (Parameter 'value')", exception.Message);
        Assert.Equal("value", exception.ParamName);
    }
}