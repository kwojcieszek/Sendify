using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class Pattern
{
    [BsonId]
    public string Id { get; set; }
    public bool IsSender { get; set;}
    public bool IsRecipient { get; set; }
    public MessageType MessageType { get; set; }
    public string Value { get; set; }
    public string Note { get; set; }
}