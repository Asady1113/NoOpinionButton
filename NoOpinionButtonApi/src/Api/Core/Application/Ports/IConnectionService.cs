using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Domain.Entities;

namespace Core.Application.Ports;

public interface IConnectionService
{
    /// <summary>
    /// WebSocket接続を処理する
    /// </summary>
    /// <param name="request">接続リクエスト</param>
    /// <returns>接続レスポンス</returns>
    Task<ConnectServiceResponse> ConnectAsync(ConnectServiceRequest request);
    
    /// <summary>
    /// WebSocket切断を処理する
    /// </summary>
    /// <param name="request">切断リクエスト</param>
    /// <returns>切断レスポンス</returns>
    Task<DisconnectServiceResponse> DisconnectAsync(DisconnectServiceRequest request);
    
    /// <summary>
    /// 会議の有効な接続一覧を取得する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <returns>有効な接続一覧</returns>
    Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId);
}