using Core.Domain.Entities;

namespace Core.Domain.Logics;

public class MeetingLogic
{
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