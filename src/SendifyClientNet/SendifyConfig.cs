namespace SendifyClientNet;

public sealed record SendifyConfig(
    string Host,
    string Token,
    double Timeout = 15.0,
    int Retries = 2,
    double BackoffSeconds = 0.6,
    bool VerifySsl = false);
