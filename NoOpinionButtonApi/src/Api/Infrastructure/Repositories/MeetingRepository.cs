using Amazon.DynamoDBv2.DataModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Meeting;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class MeetingRepository : IMeetingRepository
{
    private readonly IDynamoDBContext _context;

    public MeetingRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Meeting> GetMeetingByIdAsync(MeetingId id)
    {
        MeetingEntity meetingEntity = await _context.LoadAsync<MeetingEntity>(id.Value);

        if (meetingEntity == null)
        {
            throw new KeyNotFoundException($"Meeting with Id '{id}' was not found.");
        }

        var meeting = new Meeting(
            meetingEntity.Id,
            meetingEntity.Name,
            meetingEntity.FacilitatorPassword,
            meetingEntity.ParticipantPassword
        );

        return meeting;
    }
}