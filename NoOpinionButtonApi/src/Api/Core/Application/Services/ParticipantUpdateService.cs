using Core.Application.Ports;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Participant;

namespace Core.Application.Services;

/// <summary>
/// 参加者情報更新サービス
/// </summary>
public class ParticipantUpdateService : IParticipantUpdateService
{
    private readonly IParticipantRepository _participantRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="participantRepository">参加者リポジトリ</param>
    public ParticipantUpdateService(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
    }

    /// <inheritdoc/>
    public async Task<ParticipantUpdateServiceResponse> UpdateParticipantNameAsync(ParticipantUpdateServiceRequest request)
    {
        ParticipantId id = request.ParticipantId;
        ParticipantName name = request.Name;

        // 参加者を取得
        var participant = await _participantRepository.GetByIdAsync(id);
        if (participant == null)
        {
            throw new KeyNotFoundException($"参加者ID '{id}' が見つかりません");
        }

        // アクティブ状態確認
        if (!participant.IsActive)
        {
            throw new InvalidOperationException("非アクティブな参加者の名前は変更できません");
        }

        // 名前を更新した新しい参加者エンティティを作成
        var updatedParticipant = new Participant(
            participant.Id,
            name,
            participant.MeetingId,
            participant.NoOpinionPoint,
            participant.HasOpinion,
            participant.IsActive
        );

        // 参加者を更新
        await _participantRepository.UpdateAsync(updatedParticipant);

        // 成功レスポンスを返却
        return new ParticipantUpdateServiceResponse
        {
            UpdatedName = name
        };
    }
}