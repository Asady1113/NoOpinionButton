namespace Core.Domain.ValueObjects.Participant;

/// <summary>
/// 参加者IDを表す値オブジェクト
/// </summary>
public readonly record struct ParticipantId
{
    /// <summary>
    /// ID値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">ID値</param>
    /// <exception cref="ArgumentException">無効なID値の場合</exception>
    public ParticipantId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参加者IDは空にできません", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">ID値</param>
    public static implicit operator ParticipantId(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="participantId">参加者ID</param>
    public static implicit operator string(ParticipantId participantId) => participantId.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>ID値</returns>
    public override string ToString() => Value;
}