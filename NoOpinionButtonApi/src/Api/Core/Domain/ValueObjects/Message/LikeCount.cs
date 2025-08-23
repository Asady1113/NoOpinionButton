namespace Core.Domain.ValueObjects.Message;

/// <summary>
/// いいね数を表す値オブジェクト
/// </summary>
public readonly record struct LikeCount
{
    /// <summary>
    /// いいね数の値
    /// </summary>
    public int Value { get; } = 0;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">いいね数</param>
    /// <exception cref="ArgumentException">無効ないいね数の場合</exception>
    public LikeCount(int value)
    {
        if (value < 0)
            throw new ArgumentException("いいね数は0以上である必要があります", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 整数から暗黙的変換
    /// </summary>
    /// <param name="value">いいね数</param>
    public static implicit operator LikeCount(int value) => new(value);

    /// <summary>
    /// 整数への暗黙的変換
    /// </summary>
    /// <param name="likeCount">いいね数</param>
    public static implicit operator int(LikeCount likeCount) => likeCount.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>いいね数</returns>
    public override string ToString() => Value.ToString();
}