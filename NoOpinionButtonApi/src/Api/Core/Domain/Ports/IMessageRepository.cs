using Core.Domain.Entities;

namespace Core.Domain.Ports
{
    public interface IMessageRepository
    {
        /// <summary>
        /// メッセージを保存する
        /// </summary>
        /// <param name="message">保存するメッセージ</param>
        /// <returns>保存されたメッセージ</returns>
        Task<Message> SaveAsync(Message message);
    }
}