namespace Core.Application;

public class SignInServiceResponse
{
    public string Id { get; set; } = "";
    public string MeetingId { get; set; } = "";
    public string MeetingName { get; set; } = "";
    public bool IsFacilitator { get; set; } = false;
}