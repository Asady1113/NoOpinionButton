using Core.Domain.Entities;

namespace Core.Domain.Logics;

public class MeetingLogic
{
    /// <summary>
    /// Meetingのパスワードを判定する
    /// </summary>
    /// <param name="input">パスワード/param>
    /// <param name="meeting">Meeting</param>
    /// <returns>一致しているかどうか（一致：true, 不一致：false）</returns>
    public bool VerifyPassword(string input, Meeting meeting)
    {
        if (input == meeting.ParticipantPassword)
        {
            return true;
        }
        else if (input == meeting.FacilitatorPassword)
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