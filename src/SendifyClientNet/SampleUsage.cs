using SendifyClientNet;

// Global sensitive configuration (replace placeholders before running)
// e.g. "https://sendify.test.com"
var sendifyHost = "https://sendify.test.com";
// PLACEHOLDER: set your real token here
var sendifyToken = "<REDACTED_TOKEN>";
//# e.g. ["48600600600"]
string[] smsRecipients = ["48600600600"];
//# optional, e.g. "48600601601"
var smsSender = "48600601601";
//# e.g. ["user@example.com"]
string[] emailRecipients = ["example1@example.com", "example2@example.pl"];
var emailSender = "mailer@example.com";
//e.g. r"c:\sample-local-pdf.pdf"
var attachmentPath = "c:\\sample-local-pdf.pdf";

// Replace values with configuration from appsettings or secrets store in production.
var cfg = new SendifyConfig(
    Host: sendifyHost,
    Token: sendifyToken,
    Timeout: 15.0,
    Retries: 2,
    BackoffSeconds: 0.6,
    VerifySsl: true
);

using var client = new SendifyClient(cfg);

try
{
    var result = await client.SendMessageAsync(
        messageType: 1,
        sender: smsSender,
        recipients: smsRecipients,
        body: $"Hello from .NET at {DateTime.UtcNow:O}"
    ).ConfigureAwait(false);

    Console.WriteLine($"Send result: {result.Result} {result.Code}");

    //Send an email message
    result = await client.SendMessageAsync(
        messageType: 2,
        sender: emailSender,
        recipients: emailRecipients,
        body: $"Hello from .NET at {DateTime.UtcNow:O}",
        subject: $"Test Hello from .NET at {DateTime.UtcNow:O}",
        attachments: [Attachment.FromFile(attachmentPath)],
        isSeparate: true
    ).ConfigureAwait(false);

    Console.WriteLine($"Send result: {result.Result} {result.Code}");
}
catch (SendifyHttpException hex)
{
    Console.WriteLine($"HTTP error: {hex.StatusCode} - {hex.Message}");
    throw;
}
catch (SendifyRequestException rex)
{
    Console.WriteLine("Request error: " + rex.Message);
    throw;
}