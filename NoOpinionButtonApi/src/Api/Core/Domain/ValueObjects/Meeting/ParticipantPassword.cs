namespace Core.Domain.ValueObjects.Meeting;

/// <summary>
/// 参加者用パスワードを表す値オブジェクト
/// </summary>
public readonly record struct ParticipantPassword
{
    /// <summary>
    /// パスワードの最小長
    /// </summary>
    public const int MinLength = 4;

    /// <summary>
    /// パスワードの最大長
    /// </summary>
    public const int MaxLength = 20;

    /// <summary>
    /// パスワード値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">パスワード値</param>
    /// <exception cref="ArgumentException">無効なパスワードの場合</exception>
    public ParticipantPassword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参加者用パスワードは空にできません", nameof(value));

        if (value.Length < MinLength)
            throw new ArgumentException($"参加者用パスワードは{MinLength}文字以上である必要があります", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"参加者用パスワードは{MaxLength}文字以内である必要があります", nameof(value));

        // パスワードに空白文字が含まれていないことを確認
        if (value.Any(char.IsWhiteSpace))
            throw new ArgumentException("参加者用パスワードに空白文字を含めることはできません", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">パスワード値</param>
    public static implicit operator ParticipantPassword(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="participantPassword">参加者用パスワード</param>
    public static implicit operator string(ParticipantPassword participantPassword) => participantPassword.Value;

    /// <summary>
    /// 文字列表現（セキュリティのため隠蔽）
    /// </summary>
    /// <returns>マスクされた文字列</returns>
    public override string ToString() => new('*', Value.Length);
}