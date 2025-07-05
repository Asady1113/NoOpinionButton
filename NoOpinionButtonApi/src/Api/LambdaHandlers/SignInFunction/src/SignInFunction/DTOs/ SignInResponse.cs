using System.Text.Json.Serialization;

namespace SignInFunction.DTOs;

public class SignInResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("meetingId")]
    public int MeetingId { get; set; }
}