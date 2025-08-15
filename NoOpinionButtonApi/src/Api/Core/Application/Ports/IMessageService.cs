using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;

namespace Core.Application.Ports;

/// <summary>
/// メッセージサービスのインターフェース
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// メッセージを送信する
    /// </summary>
    /// <param name="request">メッセージ送信リクエスト</param>
    /// <returns>送信結果</returns>
    Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request);
}