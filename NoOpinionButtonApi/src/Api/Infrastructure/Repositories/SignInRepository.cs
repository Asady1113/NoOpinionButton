using Amazon.DynamoDBv2.DataModel;
using Core.Domain;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class SignInRepository : ISignInRepository
{
    private readonly IDynamoDBContext _context;

    public SignInRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    public async Task<Participant> SignInAsync(int meetingId, string password)
    {
        // TODO リファクタリング
        string id = Guid.NewGuid().ToString();
        var entity = new ParticipantEntity
        {
            Id = id,
            MeetingId = meetingId,
        };

        await _context.SaveAsync(entity);
    
        var participant = new Participant
        {
            Id = id,
            MeetingId = meetingId,
        };
        return participant;
    }
}
