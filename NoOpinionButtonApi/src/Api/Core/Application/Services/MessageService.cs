using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Application.Ports;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;
using System.Text.Json;

namespace Core.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConnectionRepository _connectionRepository;
        private readonly IBroadcastRepository _broadcastRepository;

        public MessageService(
            IMessageRepository messageRepository, 
            IConnectionRepository connectionRepository,
            IBroadcastRepository broadcastRepository)
        {
            _messageRepository = messageRepository;
            _connectionRepository = connectionRepository;
            _broadcastRepository = broadcastRepository;
        }

        /// <summary>
        /// メッセージを送信する
        /// </summary>
        /// <param name="request">メッセージ送信リクエスト</param>
        /// <returns>送信結果</returns>
        public async Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request)
        {
            // メッセージエンティティ作成
            var message = new Message
            {
                Id = IdGenerator.GenerateGuid(),
                MeetingId = request.MeetingId,
                ParticipantId = request.ParticipantId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow  // UTC時刻で保存
            };

            // データベースに保存
            var savedMessage = await _messageRepository.SaveAsync(message);

            // 同じ会議の参加者にメッセージを配信
            await BroadcastMessageToMeetingParticipants(request.MeetingId, savedMessage);

            // レスポンス作成（Successフラグなし）
            return new PostMessageServiceResponse
            {
                MessageId = savedMessage.Id
            };
        }

        /// <summary>
        /// 会議の参加者にメッセージを配信する
        /// </summary>
        /// <param name="meetingId">会議ID</param>
        /// <param name="message">配信するメッセージ</param>
        private async Task BroadcastMessageToMeetingParticipants(string meetingId, Message message)
        {
            // 会議の有効な接続一覧を取得
            var activeConnections = await _connectionRepository.GetActiveConnectionsByMeetingIdAsync(meetingId);
            
            // メッセージをJSON形式にシリアライズ
            var messageJson = JsonSerializer.Serialize(new
            {
                messageId = message.Id,
                meetingId = message.MeetingId,
                participantId = message.ParticipantId,
                content = message.Content,
                createdAt = message.CreatedAt
            });

            // 各接続にメッセージを配信
            var broadcastTasks = activeConnections.Select(connection => 
                _broadcastRepository.BroadcastToConnectionAsync(connection.Id, messageJson));
            
            await Task.WhenAll(broadcastTasks);
        }
    }
}