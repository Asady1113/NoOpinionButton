using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;

namespace Core.Application;

public class SignInService : ISignInService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IMeetingRepository _meetingRepository;

    public SignInService(IParticipantRepository participantRepository, IMeetingRepository meetingRepository)
    {
        _participantRepository = participantRepository;
        _meetingRepository = meetingRepository;
    }

    /// <inheritdoc/>
    public async Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request)
    {
        Meeting meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);

        PasswordType passwordType = meeting.VerifyPassword(request.Password);

        if (passwordType == PasswordType.InvalidPassword)
        {
            throw new UnauthorizedAccessException("Password is invalid");
        }

        var isFacilitator = passwordType == PasswordType.Facilitator;

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
            string id = IdGenerator.GenerateGuid();
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