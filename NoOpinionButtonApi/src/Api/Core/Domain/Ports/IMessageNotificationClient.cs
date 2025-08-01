using Core.Domain.Entities;

namespace Core.Domain.Ports;

/// <summary>
/// メッセージ通知クライアントのインターフェース
/// メッセージの配信を抽象化し、具体的な通信技術に依存しない
/// </summary>
public interface IMessageNotificationClient
{
    /// <summary>
    /// 会議の参加者にメッセージを通知する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <param name="message">通知するメッセージ</param>
    /// <returns>通知結果</returns>
    Task<bool> NotifyMessageAsync(string meetingId, Message message);
}