namespace Core.Domain.ValueObjects.Message;

/// <summary>
/// メッセージIDを表す値オブジェクト
/// </summary>
public readonly record struct MessageId
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
    public MessageId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("メッセージIDは空にできません", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">ID値</param>
    public static implicit operator MessageId(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="messageId">メッセージID</param>
    public static implicit operator string(MessageId messageId) => messageId.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>ID値</returns>
    public override string ToString() => Value;
}