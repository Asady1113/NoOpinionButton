namespace Core.Domain.ValueObjects.Meeting;

/// <summary>
/// 会議IDを表す値オブジェクト
/// </summary>
public readonly record struct MeetingId
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
    public MeetingId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("会議IDは空にできません", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">ID値</param>
    public static implicit operator MeetingId(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    public static implicit operator string(MeetingId meetingId) => meetingId.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>ID値</returns>
    public override string ToString() => Value;
}