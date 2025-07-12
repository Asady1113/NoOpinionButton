namespace Common.Utilities;

public static class IdGenerator
{
    /// <summary>
    /// IDを生成するメソッド
    /// </summary>
    /// <returns>生成されたID</returns>
    public static string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }
}