namespace Core.Domain.Entities;
public class Participant
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string MeetingId { get; set; } = "";
    public int NoOpinionPoint { get; set; } = 0;
    public bool HasOpinion { get; set; } = true;
    public bool IsActive { get; set; } = true;
}