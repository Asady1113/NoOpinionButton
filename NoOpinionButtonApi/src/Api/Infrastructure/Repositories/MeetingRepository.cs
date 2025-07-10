using Amazon.DynamoDBv2.DataModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class MeetingRepository : IMeetingRepository
{
    private readonly IDynamoDBContext _context;

    public MeetingRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    // </inheridoc>
    public async Task<Meeting> GetMeetingByIdAsync(string id)
    {
        MeetingEntity meetingEntity = await _context.LoadAsync<MeetingEntity>(id);

        if (meetingEntity == null)
        {
            throw new KeyNotFoundException($"Meeting with Id '{id}' was not found.");
        }

        var meeting = new Meeting
        {
            Id = meetingEntity.Id,
            Name = meetingEntity.Name,
            FacilitatorPassword = meetingEntity.FacilitatorPassword,
            ParticipantPassword = meetingEntity.ParticipantPassword,
        };

        return meeting;
    }
}