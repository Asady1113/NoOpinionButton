using System.Text.Json.Serialization;

namespace UpdateParticipantNameFunction.DTOs;

/// <summary>
/// 参加者名前更新レスポンス
/// </summary>
public class UpdateParticipantNameResponse
{
    /// <summary>
    /// 更新された名前
    /// </summary>
    [JsonPropertyName("updatedName")]
    public string UpdatedName { get; set; } = "";
}