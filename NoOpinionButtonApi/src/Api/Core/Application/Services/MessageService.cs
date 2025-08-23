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

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        /// <inheritdoc/>
        public async Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request)
        {
            // メッセージエンティティ作成
            var message = new Message(
                IdGenerator.GenerateGuid(),
                request.MeetingId,
                request.ParticipantId,
                request.Content,
                DateTime.UtcNow
            );

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