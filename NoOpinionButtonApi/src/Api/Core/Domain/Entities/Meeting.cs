using Core.Domain.ValueObjects.Meeting;

namespace Core.Domain.Entities;

public class Meeting
{
    /// <summary>
    /// ミーティングID（主キー）
    /// </summary>
    public MeetingId Id { get; private set; }

    /// <summary>
    /// ミーティング名
    /// </summary>
    public MeetingName Name { get; private set; }

    /// <summary>
    /// 司会者用パスワード
    /// </summary>
    public FacilitatorPassword FacilitatorPassword { get; private set; }

    /// <summary>
    /// 参加者用パスワード
    /// </summary>
    public ParticipantPassword ParticipantPassword { get; private set; }

    /// <summary>
    /// コンストラクタ（不変条件を強制）
    /// </summary>
    /// <param name="id">ミーティングID</param>
    /// <param name="name">ミーティング名</param>
    /// <param name="facilitatorPassword">司会者用パスワード</param>
    /// <param name="participantPassword">参加者用パスワード</param>
    /// <exception cref="ArgumentException">不正な引数の場合</exception>
    public Meeting(MeetingId id, MeetingName name, FacilitatorPassword facilitatorPassword, ParticipantPassword participantPassword)
    {
        // パスワード間の比較検証はエンティティ内に残す
        if (facilitatorPassword.Value == participantPassword.Value)
            throw new ArgumentException("司会者用パスワードと参加者用パスワードは異なる必要があります");

        Id = id;
        Name = name;
        FacilitatorPassword = facilitatorPassword;
        ParticipantPassword = participantPassword;
    }

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