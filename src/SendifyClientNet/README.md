# SendifyClientNet

SendifyClientNet is a lightweight **.NET Standard 2.0** client library for sending SMS and email messages via a Sendify HTTP service.

## Highlights
- Target: **.NET Standard 2.0**, **C# 7.3**
- Async API: `SendifyClient.SendMessageAsync(...)`
- Retries + backoff for transient errors (HTTP **429** and **5xx**)
- Attachments: base64-encoded via `Attachment.FromFile(...)`
- Return type: `SendifyResult` (`Result` + HTTP `Code`)

## Install
- Add a project reference to `SendifyClientNet`, or consume the NuGet package when published:

```bash
dotnet add package Nextbit.SendifyClientNet
```

## Minimal usage

```csharp
using System;
using System.Threading.Tasks;
using SendifyClientNet;

internal static class Program
{
    private static async Task Main()
    {
        var cfg = new SendifyConfig(
            host: "https://api.sendify.local",
            token: "YOUR_TOKEN",
            timeout: 15.0,
            retries: 2,
            backoffSeconds: 0.6,
            verifySsl: true);

        using (var client = new SendifyClient(cfg))
        {
            var result = await client.SendMessageAsync(
                messageType: 1,
                sender: "",
                recipients: new[] { "+48123456789" },
                body: "Hello from SendifyClientNet");

            Console.WriteLine($"Result: {result.Result}, Code: {result.Code}");
        }
    }
}
```

## Notes
- `VerifySsl` should remain `true` in production.
- Non-2xx responses throw `SendifyHttpException`; networking/timeouts throw `SendifyRequestException`.

## License
MIT (see repository `LICENSE`).
