using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.Logics;

namespace Core.Application;

public class SignInService : ISignInService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ParticipantLogic participantLogic = new ParticipantLogic();

    public SignInService(IParticipantRepository participantRepository, IMeetingRepository meetingRepository)
    {
        _participantRepository = participantRepository;
        _meetingRepository = meetingRepository;
    }

    // </inheridoc>
    public async Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request)
    {
        bool isCorrectPassword = await VerifyPasswordAsync(request);

        if (isCorrectPassword == false)
        {
            throw new ArgumentException("password is invalid");
        }

        string id = participantLogic.GenerateId();
        Participant participant = await _participantRepository.SaveParticipantAsync(id, request.MeetingId);

        var response = new SignInServiceResponse
        {
            Id = participant.Id,
            MeetingId = participant.MeetingId,
            MeetingName = "MockName"
        };

        return response;
    }

    private async Task<bool> VerifyPasswordAsync(SignInServiceRequest request)
    {
        Meeting meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);

        if (meeting == null)
        {
            throw new ArgumentException("MeetingId is invalid");
        }

        if (request.Password == meeting.ParticipantsPassword)
        {
            return true;
        }
        else if (request.Password == meeting.FacilitatorPassword)
        {
            // TODO; 司会者の処理
            return true;
        }
        else
        {
            return false;
        }
    }

}