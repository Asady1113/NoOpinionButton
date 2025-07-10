using Core.Domain.Entities;

namespace Core.Domain.Logics;

public class MeetingLogic
{
    /// <summary>
    /// Meetingのパスワードを判定する
    /// </summary>
    /// <param name="input">パスワード/param>
    /// <param name="meeting">Meeting</param>
    /// <returns>結果（Participant：参加者、Facilitator：司会者、InvalidPassword：不正なパスワード）</returns>
    public PasswordType VerifyPassword(string input, Meeting meeting)
    {
        if (input == meeting.FacilitatorPassword)
        {
            return PasswordType.Facilitator;
        }
        else if (input == meeting.ParticipantPassword)
        {
            // TODO; 司会者の処理
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