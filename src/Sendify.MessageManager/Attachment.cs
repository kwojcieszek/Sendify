namespace Sendify.MessageManager;

public class Attachment
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public byte[] Content { get; set; } = null!;
}