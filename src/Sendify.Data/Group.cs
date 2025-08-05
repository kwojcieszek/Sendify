using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class Group
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}