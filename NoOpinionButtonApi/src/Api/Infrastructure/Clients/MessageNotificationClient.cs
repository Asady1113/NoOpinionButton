using Core.Domain.Entities;
using Core.Domain.Ports;
using Infrastructure.Interfaces;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Clients;

/// <summary>
/// メッセージ通知クライアント
/// WebSocket APIを使用してメッセージを参加者に通知する
/// </summary>
public class MessageNotificationClient : IMessageNotificationClient
{
    private readonly IWebSocketConnectionRepository _connectionRepository;

    public MessageNotificationClient(IWebSocketConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    /// <summary>
    /// 会議の参加者にメッセージを通知する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <param name="message">通知するメッセージ</param>
    /// <returns>通知結果</returns>
    public async Task<bool> NotifyMessageAsync(string meetingId, Message message)
    {
        // 会議の有効な接続を取得
        var activeConnections = await _connectionRepository.GetActiveConnectionsByMeetingIdAsync(meetingId);
        
        if (!activeConnections.Any())
            return true; // 接続がない場合は成功として扱う

        // メッセージをJSON形式に変換
        var messageJson = JsonSerializer.Serialize(new
        {
            type = "message",
            data = new
            {
                id = message.Id,
                meetingId = message.MeetingId,
                participantId = message.ParticipantId,
                content = message.Content,
                createdAt = message.CreatedAt
            }
        });

        // 実際のWebSocket送信は後で実装（現在はスタブ）
        // TODO: WebSocketClientを使用してメッセージを送信
        return true;
    }
}