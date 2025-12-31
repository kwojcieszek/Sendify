using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendifyClientNet
{
    public sealed class SendifyClient : IDisposable
    {
        private readonly SendifyConfig _config;
        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;

        public SendifyClient(SendifyConfig config)
            : this(config, null)
        {
        }

        public SendifyClient(SendifyConfig config, HttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (httpClient == null)
            {
                var handler = new HttpClientHandler();

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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.Token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string NormalizeHost(string host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            string trimmed = host.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return "https://" + trimmed;
        }

        public Task<SendifyResult> SendMessageAsync(
            int messageType,
            string sender,
            string[] recipients,
            string body,
            string subject = null,
            Attachment[] attachments = null,
            bool? isSeparate = null,
            int priority = 9,
            int sendingStatus = 1,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (recipients == null || recipients.Length == 0)
            {
                throw new ArgumentException("recipients must contain at least one entry", nameof(recipients));
            }

            var payload = new Dictionary<string, object>
            {
                ["MessageType"] = messageType,
                ["Recipients"] = recipients,
                ["Priority"] = priority,
                ["Body"] = body
            };

            if (!string.IsNullOrEmpty(sender))
            {
                payload["Sender"] = sender;
            }

            if (!string.IsNullOrEmpty(subject))
            {
                payload["Subject"] = subject;
            }

            if (attachments != null && attachments.Length > 0)
            {
                var list = new List<Dictionary<string, object>>(attachments.Length);
                for (int i = 0; i < attachments.Length; i++)
                {
                    Attachment a = attachments[i];
                    list.Add(new Dictionary<string, object>
                    {
                        ["FileName"] = a.FileName,
                        ["ContentType"] = a.ContentType,
                        ["Content"] = a.Content
                    });
                }

                payload["Attachments"] = list;
            }

            if (isSeparate.HasValue)
            {
                payload["IsSeparate"] = isSeparate.Value;
            }

            payload["SendingStatus"] = sendingStatus;

            return PostMessageWithRetriesAsync(payload, cancellationToken);
        }

        private async Task<SendifyResult> PostMessageWithRetriesAsync(Dictionary<string, object> payload, CancellationToken cancellationToken)
        {
            Exception lastException = null;
            int attempts = _config.Retries + 1;

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/messages"))
                    {
                        string json = MinimalJson.Serialize(payload);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        using (HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                        {
                            int code = (int)response.StatusCode;

                            if (code >= 200 && code < 300)
                            {
                                return new SendifyResult(true, code);
                            }

                            if ((response.StatusCode == (HttpStatusCode)429 || (code >= 500 && code <= 599)) && attempt < attempts - 1)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                                continue;
                            }

                            string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new SendifyHttpException(code, "HTTP " + code.ToString(CultureInfo.InvariantCulture) + " calling /api/v1/messages: " + text);
                        }
                    }
                }
                catch (OperationCanceledException oce) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = oce;
                    if (attempt < attempts - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    throw new SendifyRequestException("Request timed out calling /api/v1/messages: " + oce.Message, oce);
                }
                catch (HttpRequestException hre)
                {
                    lastException = hre;
                    if (attempt < attempts - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_config.BackoffSeconds * (attempt + 1)), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    throw new SendifyRequestException("Request error calling /api/v1/messages: " + hre.Message, hre);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    throw;
                }
            }

            throw new SendifyRequestException(
                "Request failed calling /api/v1/messages: " + (lastException != null ? lastException.Message : string.Empty),
                lastException);
        }

        public void Dispose()
        {
            if (_disposeClient)
            {
                _httpClient.Dispose();
            }
        }

        private static class MinimalJson
        {
            public static string Serialize(object value)
            {
                var sb = new StringBuilder(512);
                WriteValue(sb, value);
                return sb.ToString();
            }

            private static void WriteValue(StringBuilder sb, object value)
            {
                if (value == null)
                {
                    sb.Append("null");
                    return;
                }

                var s = value as string;
                if (s != null)
                {
                    WriteString(sb, s);
                    return;
                }

                if (value is bool)
                {
                    sb.Append((bool)value ? "true" : "false");
                    return;
                }

                if (value is int)
                {
                    sb.Append(((int)value).ToString(CultureInfo.InvariantCulture));
                    return;
                }

                if (value is long)
                {
                    sb.Append(((long)value).ToString(CultureInfo.InvariantCulture));
                    return;
                }

                if (value is double)
                {
                    sb.Append(((double)value).ToString("R", CultureInfo.InvariantCulture));
                    return;
                }

                var obj = value as Dictionary<string, object>;
                if (obj != null)
                {
                    WriteObject(sb, obj);
                    return;
                }

                var arr = value as string[];
                if (arr != null)
                {
                    sb.Append('[');
                    for (int idx = 0; idx < arr.Length; idx++)
                    {
                        if (idx > 0)
                        {
                            sb.Append(',');
                        }

                        WriteString(sb, arr[idx] ?? string.Empty);
                    }
                    sb.Append(']');
                    return;
                }

                var listObj = value as List<Dictionary<string, object>>;
                if (listObj != null)
                {
                    sb.Append('[');
                    for (int idx = 0; idx < listObj.Count; idx++)
                    {
                        if (idx > 0)
                        {
                            sb.Append(',');
                        }

                        WriteObject(sb, listObj[idx]);
                    }
                    sb.Append(']');
                    return;
                }

                WriteString(sb, value.ToString());
            }

            private static void WriteObject(StringBuilder sb, Dictionary<string, object> obj)
            {
                sb.Append('{');
                bool first = true;

                foreach (KeyValuePair<string, object> kv in obj)
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }

                    first = false;

                    WriteString(sb, kv.Key);
                    sb.Append(':');
                    WriteValue(sb, kv.Value);
                }

                sb.Append('}');
            }

            private static void WriteString(StringBuilder sb, string s)
            {
                sb.Append('"');
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '"':
                            sb.Append("\\\"");
                            break;
                        case '\b':
                            sb.Append("\\b");
                            break;
                        case '\f':
                            sb.Append("\\f");
                            break;
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        default:
                            if (c < 0x20)
                            {
                                sb.Append("\\u");
                                sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }
                sb.Append('"');
            }
        }
    }
}
