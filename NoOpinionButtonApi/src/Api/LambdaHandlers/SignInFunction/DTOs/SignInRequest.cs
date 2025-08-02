using System.Text.Json.Serialization;

namespace SignInFunction.DTOs;

public class SignInRequest
{
    [JsonPropertyName("meetingId")]
    public string MeetingId { get; set; } = "";
    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
}