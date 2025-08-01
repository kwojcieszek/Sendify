using Sendify.Data;

namespace Sendify.Api.Models;

public class MessageToSend
{
    public MessageType MessageType { get; set; }
    public string Sender { get; set; } = string.Empty;
    public ICollection<string>? Recipients { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ICollection<Attachment>? Attachments { get; set; }
    public bool IsSeparate { get; set; }
}