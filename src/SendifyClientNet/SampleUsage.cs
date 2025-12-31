using System;
using System.Threading.Tasks;

namespace SendifyClientNet
{
    internal static class SampleUsage
    {
        private static async Task Main()
        {
            // Replace placeholders before running.
            var sendifyHost = "https://sendify.test.com";
            var sendifyToken = "<REDACTED_TOKEN>";

            string[] smsRecipients = new[] { "48600600600" };
            var smsSender = "";

            string[] emailRecipients = new[] { "example1@example.com", "example2@example.pl" };
            var emailSender = "mailer@example.com";

            var cfg = new SendifyConfig(sendifyHost, sendifyToken, timeout: 15.0, retries: 2, backoffSeconds: 0.6, verifySsl: true);

            using (var client = new SendifyClient(cfg))
            {
                try
                {
                    var result = await client.SendMessageAsync(
                        messageType: 1,
                        sender: smsSender,
                        recipients: smsRecipients,
                        body: "Hello from .NET at " + DateTime.UtcNow.ToString("O")).ConfigureAwait(false);

                    Console.WriteLine("Send result: " + result.Result + " " + result.Code);

                    // Email example
                    result = await client.SendMessageAsync(
                        messageType: 2,
                        sender: emailSender,
                        recipients: emailRecipients,
                        body: "Hello from .NET at " + DateTime.UtcNow.ToString("O"),
                        subject: "Test " + DateTime.UtcNow.ToString("O"),
                        attachments: null,
                        isSeparate: true).ConfigureAwait(false);

                    Console.WriteLine("Send result: " + result.Result + " " + result.Code);
                }
                catch (SendifyHttpException e)
                {
                    Console.WriteLine("HTTP error: " + e.StatusCode + " - " + e.Message);
                }
                catch (SendifyRequestException e)
                {
                    Console.WriteLine("Request error: " + e.Message);
                }
            }
        }
    }
}