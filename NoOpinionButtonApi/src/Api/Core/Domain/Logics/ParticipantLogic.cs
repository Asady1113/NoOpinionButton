namespace Core.Domain.Logics;

public class ParticipantLogic
{
    /// <summary>
    /// IDを生成するメソッド
    /// </summary>
    /// <returns>生成されたID</returns>
    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}