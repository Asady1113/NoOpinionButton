using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.Logics;

namespace Core.Application;

public class SignInService : ISignInService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ParticipantLogic _participantLogic = new ParticipantLogic();
    private readonly MeetingLogic _meetingLogic = new MeetingLogic();

    public SignInService(IParticipantRepository participantRepository, IMeetingRepository meetingRepository)
    {
        _participantRepository = participantRepository;
        _meetingRepository = meetingRepository;
    }

    // </inheridoc>
    public async Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request)
    {
        Meeting meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);

        PasswordType passwordType = _meetingLogic.VerifyPassword(request.Password, meeting);

        if (passwordType == PasswordType.InvalidPassword)
        {
            throw new KeyNotFoundException("password is invalid");
        }

        var isFacilitator = passwordType == PasswordType.Facilitator ? true : false;

        if (isFacilitator)
        {
            var response = new SignInServiceResponse
            {
                MeetingId = meeting.Id,
                MeetingName = meeting.Name,
                IsFacilitator = isFacilitator,
            };
            return response;
        }
        else
        {
            string id = _participantLogic.GenerateId();
            Participant participant = await _participantRepository.SaveParticipantAsync(id, request.MeetingId);

            var response = new SignInServiceResponse
            {
                Id = participant.Id,
                MeetingId = participant.MeetingId,
                MeetingName = meeting.Name,
                IsFacilitator = isFacilitator,
            };

            return response;
        }
    }

}