namespace Core.Domain.ValueObjects.Message;

/// <summary>
/// 通報回数を表す値オブジェクト
/// </summary>
public readonly record struct ReportedCount
{
    /// <summary>
    /// 通報回数の値
    /// </summary>
    public int Value { get; } = 0;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">通報回数</param>
    /// <exception cref="ArgumentException">無効な通報回数の場合</exception>
    public ReportedCount(int value)
    {
        if (value < 0)
            throw new ArgumentException("通報回数は0以上である必要があります", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 整数から暗黙的変換
    /// </summary>
    /// <param name="value">通報回数</param>
    public static implicit operator ReportedCount(int value) => new(value);

    /// <summary>
    /// 整数への暗黙的変換
    /// </summary>
    /// <param name="reportedCount">通報回数</param>
    public static implicit operator int(ReportedCount reportedCount) => reportedCount.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>通報回数</returns>
    public override string ToString() => Value.ToString();
}