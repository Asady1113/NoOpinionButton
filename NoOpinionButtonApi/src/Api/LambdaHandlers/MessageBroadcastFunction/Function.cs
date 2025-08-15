using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.DependencyInjection;
using DependencyInjection;
using Core.Application.Ports;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MessageBroadcastFunction;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBroadcastService _broadcastService;

    public Function()
    {
        _serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        _broadcastService = _serviceProvider.GetRequiredService<IBroadcastService>();
    }

    /// <summary>
    /// DynamoDB Streamsイベントを処理してメッセージを配信する
    /// </summary>
    /// <param name="dynamoEvent">DynamoDB Streamsイベント</param>
    /// <param name="context">Lambda実行コンテキスト</param>
    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        foreach (var record in dynamoEvent.Records)
        {
            context.Logger.LogLine($"Processing record: {record.EventName}");

            // INSERTイベントのみ処理（新しいメッセージが保存された場合）
            if (record.EventName == "INSERT")
            {
                await ProcessMessageInsert(record, context);
            }
        }
    }

    /// <summary>
    /// メッセージ挿入イベントを処理する
    /// </summary>
    private async Task ProcessMessageInsert(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
    {
        try
        {
            // DynamoDB StreamのNewImageからメッセージ情報を取得
            var newImage = record.Dynamodb.NewImage;
            
            var messageId = newImage["Id"].S;
            var meetingId = newImage["MeetingId"].S;
            var participantId = newImage["ParticipantId"].S;
            var content = newImage["Content"].S;
            var createdAt = DateTime.Parse(newImage["CreatedAt"].S);

            context.Logger.LogLine($"Broadcasting message {messageId} to meeting {meetingId}");
            
            // メッセージをJSON形式にシリアライズ
            var messageJson = JsonSerializer.Serialize(new
            {
                messageId,
                meetingId,
                participantId,
                content,
                createdAt
            });

            // BroadcastServiceを経由してメッセージを配信
            await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);
            
            context.Logger.LogLine($"Message broadcast completed");
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error processing message insert: {ex}");
            throw;
        }
    }
}
