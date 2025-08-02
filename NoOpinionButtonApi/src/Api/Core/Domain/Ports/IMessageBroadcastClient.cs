using Core.Domain.Entities;

namespace Core.Domain.Ports;

/// <summary>
/// メッセージ配信クライアントのインターフェース
/// メッセージの配信を抽象化し、具体的な通信技術に依存しない
/// </summary>
public interface IMessageBroadcastClient
{
    /// <summary>
    /// 会議の参加者にメッセージを配信する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <param name="message">配信するメッセージ</param>
    /// <returns>配信結果</returns>
    Task<bool> BroadcastMessageAsync(string meetingId, Message message);
}