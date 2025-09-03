using Core.Domain.Entities;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;

namespace Core.Domain.Ports;

public interface IParticipantRepository
{
    /// <summary>
    /// ParticipantEntityを保存するメソッド
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="meetingId">MeetingId</param>
    /// <returns>保存されたParticipant</returns>
    Task<Participant> SaveParticipantAsync(ParticipantId id, MeetingId meetingId);
}