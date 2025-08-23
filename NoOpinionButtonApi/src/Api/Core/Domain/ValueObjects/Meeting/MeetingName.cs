namespace Core.Domain.ValueObjects.Meeting;

/// <summary>
/// ミーティング名を表す値オブジェクト
/// </summary>
public readonly record struct MeetingName
{
    /// <summary>
    /// ミーティング名の最大長
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// 名前値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">名前値</param>
    /// <exception cref="ArgumentException">無効な名前の場合</exception>
    public MeetingName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ミーティング名は空にできません", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"ミーティング名は{MaxLength}文字以内である必要があります", nameof(value));

        Value = value.Trim();
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">名前値</param>
    public static implicit operator MeetingName(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="meetingName">ミーティング名</param>
    public static implicit operator string(MeetingName meetingName) => meetingName.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>名前値</returns>
    public override string ToString() => Value;
}