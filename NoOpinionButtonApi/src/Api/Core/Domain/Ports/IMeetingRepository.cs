using Core.Domain.Entities;
using Core.Domain.ValueObjects.Meeting;

namespace Core.Domain.Ports;

public interface IMeetingRepository
{
    /// <summary>
    /// Idと一致するMeetingを取得
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns>Idと一致するMeeting</returns>
    Task<Meeting> GetMeetingByIdAsync(MeetingId id);
}