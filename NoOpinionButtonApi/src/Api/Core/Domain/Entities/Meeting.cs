namespace Core.Domain.Entities;

public class Meeting
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FacilitatorPassword { get; set; }
    public string ParticipantsPassword { get; set; }
}