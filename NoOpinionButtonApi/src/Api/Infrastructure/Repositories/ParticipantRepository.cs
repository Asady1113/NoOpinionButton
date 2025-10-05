using Amazon.DynamoDBv2.DataModel;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Meeting;
using Core.Domain.ValueObjects.Participant;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class ParticipantRepository : IParticipantRepository
{
    private readonly IDynamoDBContext _context;

    public ParticipantRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Participant> SaveParticipantAsync(ParticipantId id, MeetingId meetingId)
    {
        var entity = new ParticipantEntity
        {
            Id = id,
            MeetingId = meetingId,
            Name = "未設定"
        };

        await _context.SaveAsync(entity);

        var participant = new Participant(
            id,
            "未設定", // 名前のデフォルト値（今後のユースケースで更新予定）
            meetingId,
            0   // NoOpinionPointのデフォルト値
        );
        return participant;
    }

    /// <inheritdoc/>
    public async Task<Participant?> GetByIdAsync(ParticipantId id)
    {
        var entity = await _context.LoadAsync<ParticipantEntity>(id.Value);
        
        if (entity == null)
            return null;

        return new Participant(
            entity.Id,
            entity.Name,
            entity.MeetingId,
            entity.NoOpinionPoint,
            entity.HasOpinion,
            entity.IsActive
        );
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Participant participant)
    {
        var entity = new ParticipantEntity
        {
            Id = participant.Id,
            Name = participant.Name,
            MeetingId = participant.MeetingId,
            NoOpinionPoint = participant.NoOpinionPoint.Value,
            HasOpinion = participant.HasOpinion,
            IsActive = participant.IsActive
        };

        await _context.SaveAsync(entity);
    }
}
