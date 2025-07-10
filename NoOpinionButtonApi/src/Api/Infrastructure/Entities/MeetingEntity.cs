using Amazon.DynamoDBv2.DataModel;

namespace Infrastructure.Entities;

[DynamoDBTable("Meeting")]
public class MeetingEntity
{
    [DynamoDBHashKey]  // パーティションキー
    public string Id { get; set; }
    [DynamoDBProperty]
    public string Name { get; set; }
    [DynamoDBProperty]
    public string FacilitatorPassword { get; set; }
    [DynamoDBProperty]
    public string ParticipantPassword { get; set; }
}