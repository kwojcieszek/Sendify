using System;

namespace SendifyClientNet
{
    public sealed class SendifyConfig
    {
        public string Host { get; }
        public string Token { get; }
        public double Timeout { get; }
        public int Retries { get; }
        public double BackoffSeconds { get; }
        public bool VerifySsl { get; }

        public SendifyConfig(string host, string token, double timeout = 15.0, int retries = 2, double backoffSeconds = 0.6, bool verifySsl = true)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("host is required", nameof(host));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("token is required", nameof(token));
            }

            Host = host;
            Token = token;
            Timeout = timeout;
            Retries = retries;
            BackoffSeconds = backoffSeconds;
            VerifySsl = verifySsl;
        }
    }
}
