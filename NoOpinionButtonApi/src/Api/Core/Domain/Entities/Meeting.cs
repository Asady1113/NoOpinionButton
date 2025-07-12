namespace Core.Domain.Entities;

public class Meeting
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FacilitatorPassword { get; set; } = "";
    public string ParticipantPassword { get; set; } = "";

    /// <summary>
    /// Meetingのパスワードを判定する
    /// </summary>
    /// <param name="password">パスワード</param>
    /// <returns>結果（Participant：参加者、Facilitator：司会者、InvalidPassword：不正なパスワード）</returns>
    public PasswordType VerifyPassword(string password)
    {
        if (password == FacilitatorPassword)
        {
            return PasswordType.Facilitator;
        }
        else if (password == ParticipantPassword)
        {
            return PasswordType.Participant;
        }
        else
        {
            return PasswordType.InvalidPassword;
        }
    }
}

public enum PasswordType
{
    Facilitator,
    Participant,
    InvalidPassword
}