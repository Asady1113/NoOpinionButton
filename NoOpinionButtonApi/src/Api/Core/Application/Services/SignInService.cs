using Core.Domain;

namespace Core.Application;

public class SignInService : ISignInService
{
    private readonly ISignInRepository _signInRepository;

    public SignInService(ISignInRepository signInRepository)
    {
        _signInRepository = signInRepository;
    }

    // </inheridoc>
    public async Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request)
    {
        Participant participant = await _signInRepository.SignInAsync();

        var response = new SignInServiceResponse
        {
            Id = participant.Id,
            MeetingId = participant.MeetingId,
        };
        
        return response;
    }
}
