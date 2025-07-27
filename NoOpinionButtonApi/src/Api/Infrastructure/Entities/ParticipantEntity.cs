using Amazon.DynamoDBv2.DataModel;

namespace Infrastructure.Entities;

[DynamoDBTable("Participant")]
public class ParticipantEntity
{
    [DynamoDBHashKey]  // パーティションキー
    public string Id { get; set; } = "";

    [DynamoDBProperty]
    public string Name { get; set; } = "";

    [DynamoDBProperty]
    public string MeetingId { get; set; } = "";

    [DynamoDBProperty]
    public int NoOpinionPoint { get; set; } = 0;

    [DynamoDBProperty]
    public bool HasOpinion { get; set; } = true;

    [DynamoDBProperty]
    public bool IsActive { get; set; } = true;
}