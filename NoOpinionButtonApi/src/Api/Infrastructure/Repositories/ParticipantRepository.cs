using Amazon.DynamoDBv2.DataModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class ParticipantRepository : IParticipantRepository
{
    private readonly IDynamoDBContext _context;

    public ParticipantRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    // </inheridoc>
    public async Task<Participant> SaveParticipantAsync(string id, int meetingId)
    {
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
