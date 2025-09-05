using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class Token
{
    [BsonId]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string TokenHash { get; set; }
    public string? TokenName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
}