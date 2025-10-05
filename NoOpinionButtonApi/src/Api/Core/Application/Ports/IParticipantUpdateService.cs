namespace Core.Application.Ports;

/// <summary>
/// 参加者情報更新サービスのインターフェース
/// </summary>
public interface IParticipantUpdateService
{
    /// <summary>
    /// 参加者の名前を更新する
    /// </summary>
    /// <returns>更新処理の結果</returns>
    /// <exception cref="ArgumentException">参加者IDまたは名前が無効な場合</exception>
    /// <exception cref="KeyNotFoundException">参加者が存在しない場合</exception>
    /// <exception cref="InvalidOperationException">参加者が非アクティブな場合</exception>
    Task<ParticipantUpdateServiceResponse> UpdateParticipantNameAsync(ParticipantUpdateServiceRequest request);
}