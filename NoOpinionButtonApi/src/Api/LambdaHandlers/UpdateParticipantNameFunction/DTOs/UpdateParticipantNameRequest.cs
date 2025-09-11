using System.Text.Json.Serialization;

namespace UpdateParticipantNameFunction.DTOs;

/// <summary>
/// 参加者名前更新リクエスト（内部処理用）
/// </summary>
public class UpdateParticipantNameRequest
{
    
    /// <summary>
    /// 新しい名前
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}