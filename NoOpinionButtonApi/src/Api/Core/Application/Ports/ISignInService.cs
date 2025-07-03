namespace Core.Application;
public interface ISignInService
{
    /// <summary>
    /// 会議に参加するメソッド
    /// </summary>
    /// <returns>参加者の詳細データ</returns>
    Task<SignInServiceResponse> SignInAsync(SignInServiceRequest request);
}