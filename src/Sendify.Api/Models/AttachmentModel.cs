using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class AttachmentModel
{
    public string FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Content { get; set; }
}