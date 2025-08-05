using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class Message
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public MessageType MessageType { get; set; }
    public string Sender { get; set; } = string.Empty;
    public ICollection<string>? Recipients { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ICollection<Attachment>? Attachments { get; set; } = null!;
    public bool IsSeparate { get; set; } = false;
    public SendingStatus SendingStatus { get; set; } = SendingStatus.None;
    public int SendingAttempts { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FetchedAt { get; set; } = null;
    public DateTime? SentAt { get; set; } = null;
    public DateTime? FailedAt { get; set; } = null;
    public string ErrorMessage { get; set; } = string.Empty;
}