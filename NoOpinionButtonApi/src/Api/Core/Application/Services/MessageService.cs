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

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        /// <inheritdoc/>
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

            // レスポンス作成（Successフラグなし）
            return new PostMessageServiceResponse
            {
                MessageId = savedMessage.Id
            };
        }
    }
}