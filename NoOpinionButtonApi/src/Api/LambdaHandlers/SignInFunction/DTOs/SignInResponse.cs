using System.Text.Json.Serialization;

namespace SignInFunction.DTOs;

public class SignInResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("meetingId")]
    public string MeetingId { get; set; } = "";

    [JsonPropertyName("meetingName")]
    public string MeetingName { get; set; } = "";

    [JsonPropertyName("isFacilitator")]
    public bool IsFacilitator { get; set; } = false;
}