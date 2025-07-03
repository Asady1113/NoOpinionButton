using Core.Domain;

namespace Infrastructure.Repository;

public class SignInRepository : ISignInRepository
{
    public async Task<Participant> SignInAsync()
    {
        // MockData
        var participant = new Participant
        {
            Id = 1,
            Name = "MockName",
            MeetingId = 1,
        };
        return participant;
    }
}
