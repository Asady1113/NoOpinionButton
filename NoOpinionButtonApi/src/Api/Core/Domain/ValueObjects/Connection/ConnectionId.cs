namespace Core.Domain.ValueObjects.Connection;

/// <summary>
/// 接続IDを表す値オブジェクト
/// </summary>
public readonly record struct ConnectionId
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
    public ConnectionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("接続IDは空にできません", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 文字列から暗黙的変換
    /// </summary>
    /// <param name="value">ID値</param>
    public static implicit operator ConnectionId(string value) => new(value);

    /// <summary>
    /// 文字列への暗黙的変換
    /// </summary>
    /// <param name="connectionId">接続ID</param>
    public static implicit operator string(ConnectionId connectionId) => connectionId.Value;

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>ID値</returns>
    public override string ToString() => Value;
}