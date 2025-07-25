namespace Sendify.MessageService;

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string[] Recipients { get; set; } = null!;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = null!;
    public Attachment[] Attachments { get; set; } = null!;
    public bool IsSeparate { get; set; } = false;
}