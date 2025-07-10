namespace Core.Domain.Entities;

public class Meeting
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string FacilitatorPassword { get; set; }
    public string ParticipantPassword { get; set; }
}