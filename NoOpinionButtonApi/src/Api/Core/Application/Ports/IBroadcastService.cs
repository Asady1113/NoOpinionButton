namespace Core.Application.Ports;

public interface IBroadcastService
{
    /// <summary>
    /// 会議の全参加者にメッセージを配信する
    /// </summary>
    /// <param name="meetingId">会議ID</param>
    /// <param name="messageJson">配信するメッセージ（JSON形式）</param>
    Task BroadcastMessageToMeetingAsync(string meetingId, string messageJson);
}