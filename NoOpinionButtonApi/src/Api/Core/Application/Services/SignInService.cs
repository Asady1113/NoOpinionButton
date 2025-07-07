using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.Logics;

namespace Core.Application;

public class SignInService : ISignInService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly ParticipantLogic participantLogic = new ParticipantLogic();

    public SignInService(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository;
    }

    // </inheridoc>
    public async Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request)
    {
        // TODO; パスワードの判定等
        string id = participantLogic.GenerateId();
        Participant participant = await _participantRepository.SaveParticipant(id, request.MeetingId);

        var response = new SignInServiceResponse
        {
            Id = participant.Id,
            MeetingId = participant.MeetingId,
            MeetingName = "MockName"
        };
        
        return response;
    }
}
