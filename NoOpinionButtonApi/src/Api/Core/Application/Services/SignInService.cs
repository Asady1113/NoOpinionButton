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
        // TODO; パスワードの判定等
        Participant participant = await _signInRepository.SignInAsync(request.MeetingId, request.Password);

        var response = new SignInServiceResponse
        {
            Id = participant.Id,
            MeetingId = participant.MeetingId,
            MeetingName = "MockName"
        };
        
        return response;
    }
}
