using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace SendifyClientNet;

public sealed class SendifyClient : IDisposable
{
    private readonly SendifyConfig _config;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeClient;

    public SendifyClient(SendifyConfig config)
        : this(config, null)
    {
    }

    public SendifyClient(SendifyConfig config, HttpClient? httpClient)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        if (httpClient is null)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (!config.VerifySsl)
            {
                // For development/testing only: skip SSL validation when configured.
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            _httpClient = new HttpClient(handler);
            _disposeClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _disposeClient = false;
        }

        string baseUrl = NormalizeHost(_config.Host).TrimEnd('/');
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.Timeout);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.Token}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private static string NormalizeHost(string host)
    {
        string trimmed = host?.Trim() ?? throw new ArgumentNullException(nameof(host));
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return $"https://{trimmed}";
    }

    public async Task<SendifyResult> SendMessageAsync(
        int messageType,
        string? sender,
        string[] recipients,
        string body,
        string? subject = null,
        Attachment[]? attachments = null,
        bool? isSeparate = null,
        int priority = 9,
        int sendingStatus = 1,
        JsonObject? extraFields = null,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Length == 0)
        {
            throw new ArgumentException("recipients must contain at least one entry", nameof(recipients));
        }

        JsonObject payload = new JsonObject
        {
            ["MessageType"] = messageType,
            ["Recipients"] = new JsonArray(recipients.Select(r => JsonValue.Create(r)).ToArray()),
            ["Priority"] = priority,
            ["Body"] = body
        };

        if (sender is not null)
        {
            payload["Sender"] = sender;
        }

        if (subject is not null)
        {
            payload["Subject"] = subject;
        }

        if (attachments is not null && attachments.Length > 0)
        {
            JsonArray array = new JsonArray();
            foreach (Attachment a in attachments)
            {
                JsonObject o = new JsonObject
                {
                    ["FileName"] = a.FileName,
                    ["ContentType"] = a.ContentType,
                    ["Content"] = a.Content
                };
                array.Add(o);
            }

            payload["Attachments"] = array;
        }

        if (isSeparate is not null)
        {
            payload["IsSeparate"] = isSeparate.Value;
        }

        if (sendingStatus is not 0)
        {
            payload["SendingStatus"] = sendingStatus;
        }

        if (extraFields is not null)
        {
            foreach (KeyValuePair<string, JsonNode?> kv in extraFields)
            {
                payload[kv.Key] = kv.Value;
            }
        }

        return await PostMessageWithRetriesAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SendifyResult> PostMessageWithRetriesAsync(JsonObject payload, CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        int attempts = _config.Retries + 1;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/messages", payload, cancellationToken).ConfigureAwait(false);

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    return (new SendifyResult((int)response.StatusCode==200, (int)response.StatusCode));
                }

                if ((response.StatusCode == (HttpStatusCode)429 || ((int)response.StatusCode >= 500 && (int)response.StatusCode <= 599))
                    && attempt < attempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                string text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new SendifyHttpException((int)response.StatusCode, $"HTTP {(int)response.StatusCode} calling /api/v1/messages: {text}");
            }
            catch (OperationCanceledException oce) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = oce;
                if (attempt < attempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw new SendifyRequestException($"Request timed out calling /api/v1/messages: {oce.Message}", oce);
            }
            catch (HttpRequestException hre)
            {
                lastException = hre;
                if (attempt < attempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw new SendifyRequestException($"Request error calling /api/v1/messages: {hre.Message}", hre);
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        throw new SendifyRequestException($"Request failed calling /api/v1/messages: {lastException?.Message}", lastException);
    }

    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient.Dispose();
        }
    }
}
