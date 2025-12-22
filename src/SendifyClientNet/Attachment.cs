using System.Text.Json.Serialization;

namespace SendifyClientNet;

public sealed class Attachment
{
    [JsonPropertyName("FileName")]
    public string FileName { get; }

    [JsonPropertyName("ContentType")]
    public string ContentType { get; }

    [JsonPropertyName("Content")]
    public string Content { get; }

    public Attachment(string fileName, string contentType, string content)
    {
        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }

    public static Attachment FromFile(string path, string? fileName = null, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("path is required", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Attachment file not found", path);
        }

        byte[] raw = File.ReadAllBytes(path);
        string b64 = Convert.ToBase64String(raw);

        string name = fileName ?? Path.GetFileName(path);
        string ctype = contentType ?? GuessContentType(name);

        return new Attachment(name, ctype, b64);
    }

    private static string GuessContentType(string name)
    {
        // Minimal mapping; fallback to application/octet-stream
        string ext = Path.GetExtension(name).ToLowerInvariant();
        return ext switch
        {
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".json" => "application/json",
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".csv" => "text/csv",
            _ => "application/octet-stream",
        };
    }
}
