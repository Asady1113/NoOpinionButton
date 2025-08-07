using Core.Application.DTOs.Requests;
using Core.Application.DTOs.Responses;
using Core.Domain.Entities;

namespace Core.Application.Ports;

public interface IConnectionService
{
    Task<ConnectServiceResponse> ConnectAsync(ConnectServiceRequest request);
    Task<DisconnectServiceResponse> DisconnectAsync(DisconnectServiceRequest request);
    Task<IEnumerable<Connection>> GetActiveConnectionsByMeetingIdAsync(string meetingId);
}