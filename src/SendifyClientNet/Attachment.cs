using System;
using System.IO;

namespace SendifyClientNet
{
    public sealed class Attachment
    {
        public string FileName { get; }
        public string ContentType { get; }
        public string Content { get; }

        public Attachment(string fileName, string contentType, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("fileName is required", nameof(fileName));
            }

            FileName = fileName;
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            Content = content ?? string.Empty;
        }

        public static Attachment FromFile(string path, string fileName = null, string contentType = null)
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

            switch (ext)
            {
                case ".txt":
                    return "text/plain";
                case ".html":
                case ".htm":
                    return "text/html";
                case ".json":
                    return "application/json";
                case ".pdf":
                    return "application/pdf";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".csv":
                    return "text/csv";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
