namespace Core.Domain;
public class Participant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MeetingId { get; set; }
    public int NoOpinionPoint { get; set; } = 0;
    public bool HasOpinion { get; set; } = true;
    public bool IsActive { get; set; } = true;
}