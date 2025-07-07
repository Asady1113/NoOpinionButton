namespace Core.Domain;

public interface ISignInRepository
{
    Task<Participant> SignInAsync(int meetingId, string password);
}