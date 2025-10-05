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

    /// <summary>
    /// IDで参加者を取得するメソッド
    /// </summary>
    /// <param name="id">参加者ID</param>
    /// <returns>参加者エンティティ、存在しない場合はnull</returns>
    Task<Participant?> GetByIdAsync(ParticipantId id);

    /// <summary>
    /// 参加者エンティティを更新するメソッド
    /// </summary>
    /// <param name="participant">更新する参加者エンティティ</param>
    /// <returns>更新処理のタスク</returns>
    Task UpdateAsync(Participant participant);
}