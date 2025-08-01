using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class Attachment
{
    [BsonId]
    public string Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Content { get; set; }
}