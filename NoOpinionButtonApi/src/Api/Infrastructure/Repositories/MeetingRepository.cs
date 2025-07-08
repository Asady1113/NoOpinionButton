using System.Runtime.CompilerServices;
using Core.Domain.Entities;
using Core.Domain.Ports;

namespace Infrastructure.Repository;

public class MeetingRepository : IMeetingRepository
{
    // </inheridoc>
    public async Task<Meeting> GetMeetingByIdAsync(int id)
    {
        // TODO; MeetingをDBから取得する
        var meeting = new Meeting
        {
            Id = id,
            Name = "MockMeeting",
            FacilitatorPassword = "MockFacilitatorPassword",
            ParticipantsPassword = "MockParticipantPassword",
        };

        return meeting;
    }
}