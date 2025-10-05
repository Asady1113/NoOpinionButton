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
        private readonly IParticipantRepository _participantRepository;

        public MessageService(IMessageRepository messageRepository, IParticipantRepository participantRepository)
        {
            _messageRepository = messageRepository;
            _participantRepository = participantRepository;
        }

        /// <inheritdoc/>
        public async Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request)
        {
            // 参加者名を取得
            var participant = await _participantRepository.GetByIdAsync(request.ParticipantId);
            if (participant == null)
            {
                throw new ArgumentException($"参加者が見つかりません: {request.ParticipantId}");
            }

            // メッセージエンティティ作成（参加者名を含む）
            var message = new Message(
                IdGenerator.GenerateGuid(),
                request.MeetingId,
                request.ParticipantId,
                participant.Name,
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