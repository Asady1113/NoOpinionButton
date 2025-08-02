using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Application.Ports;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;

namespace Core.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageBroadcastClient _broadcastClient;

        public MessageService(IMessageRepository messageRepository, IMessageBroadcastClient broadcastClient)
        {
            _messageRepository = messageRepository;
            _broadcastClient = broadcastClient;
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

            // 参加者にメッセージを配信
          //  await _broadcastClient.BroadcastMessageAsync(request.MeetingId, savedMessage);

            // レスポンス作成（Successフラグなし）
            return new PostMessageServiceResponse
            {
                MessageId = savedMessage.Id
            };
        }
    }
}