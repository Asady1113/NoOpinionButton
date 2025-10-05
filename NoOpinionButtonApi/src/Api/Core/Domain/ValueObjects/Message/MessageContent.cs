namespace Core.Domain.ValueObjects.Message;

/// <summary>
/// メッセージ内容を表す値オブジェクト
/// </summary>
public readonly record struct MessageContent
{
    /// <summary>
    /// メッセージの最大長
    /// </summary>
    public const int MaxLength = 500;

    /// <summary>
    /// 内容値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">内容値</param>
    /// <exception cref="ArgumentException">無効な内容の場合</exception>
    public MessageContent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("メッセージ内容は空にできません", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"メッセージ内容は{MaxLength}文字以内である必要があります", nameof(value));

        Value = value.Trim();
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">内容値</param>
    public static implicit operator MessageContent(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="messageContent">メッセージ内容</param>
    public static implicit operator string(MessageContent messageContent) => messageContent.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>内容値</returns>
    public override string ToString() => Value;
}