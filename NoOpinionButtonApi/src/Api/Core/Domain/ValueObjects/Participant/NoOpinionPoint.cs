namespace Core.Domain.ValueObjects.Participant;

/// <summary>
/// 意見なしボタンを押せる回数を表す値オブジェクト
/// </summary>
public readonly record struct NoOpinionPoint
{
    /// <summary>
    /// 最大ポイント数
    /// </summary>
    public const int MaxPoint = 2;

    /// <summary>
    /// ポイント値
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">ポイント値</param>
    /// <exception cref="ArgumentException">無効なポイント値の場合</exception>
    public NoOpinionPoint(int value)
    {
        if (value < 0)
            throw new ArgumentException("意見なしポイントは0以上である必要があります", nameof(value));

        if (value > MaxPoint)
            throw new ArgumentException($"意見なしポイントは{MaxPoint}以下である必要があります", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 整数から <see cref="NoOpinionPoint"/> への暗黙的変換
    /// 例: NoOpinionPoint point = 1; // new NoOpinionPoint(1) と同じように変換される
    /// </summary>
    /// <param name="value">ポイント値</param>
    public static implicit operator NoOpinionPoint(int value) => new(value);

    /// <summary>
    /// <see cref="NoOpinionPoint"/> から整数への暗黙的変換
    /// 例: NoOpinionPoint point = new NoOpinionPoint(2);
    /// int i = point; // point.Value が自動的に取り出される（変換される）
    /// </summary>
    /// <param name="noOpinionPoint">意見なしポイント</param>
    public static implicit operator int(NoOpinionPoint noOpinionPoint) => noOpinionPoint.Value;

    /// <summary>
    /// 文字列表現
    /// 例: Console.WriteLine(point); // "2"
    /// </summary>
    /// <returns>ポイント値</returns>
    public override string ToString() => Value.ToString();
}