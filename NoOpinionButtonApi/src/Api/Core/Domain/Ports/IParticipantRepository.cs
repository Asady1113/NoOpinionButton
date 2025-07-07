using Core.Domain.Entities;

namespace Core.Domain.Ports;

public interface IParticipantRepository
{
    /// <summary>
    /// ParticipantEntityを保存するメソッド
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="meetingId">MeetingId</param>
    /// <returns>保存されたParticipant</returns>
    Task<Participant> SaveParticipant(string id, int meetingId);
}