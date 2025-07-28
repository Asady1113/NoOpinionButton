using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Common.Utilities;

namespace Core.Application.Services
{
    public class ChatService
    {
        private readonly IMessageRepository _messageRepository;

        public ChatService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        /// <summary>
        /// メッセージを送信する
        /// </summary>
        /// <param name="request">メッセージ送信リクエスト</param>
        /// <returns>送信結果</returns>
        public async Task<PostMessageResponse> PostMessageAsync(PostMessageRequest request)
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
            return new PostMessageResponse
            {
                MessageId = savedMessage.Id
            };
        }
    }
}