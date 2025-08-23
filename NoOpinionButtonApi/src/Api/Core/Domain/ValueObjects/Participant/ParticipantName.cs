namespace Core.Domain.ValueObjects.Participant;

/// <summary>
/// 参加者名を表す値オブジェクト
/// </summary>
public readonly record struct ParticipantName
{
    /// <summary>
    /// 名前の最大長
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// 名前値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">名前値</param>
    /// <exception cref="ArgumentException">無効な名前の場合</exception>
    public ParticipantName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参加者名は空にできません", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"参加者名は{MaxLength}文字以内である必要があります", nameof(value));

        Value = value.Trim();
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">名前値</param>
    public static implicit operator ParticipantName(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="participantName">参加者名</param>
    public static implicit operator string(ParticipantName participantName) => participantName.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>名前値</returns>
    public override string ToString() => Value;
}