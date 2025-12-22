namespace SendifyClientNet;

public class SendifyException : Exception
{
    public SendifyException()
    {
    }

    public SendifyException(string? message)
        : base(message)
    {
    }

    public SendifyException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}

public sealed class SendifyHttpException : SendifyException
{
    public int StatusCode { get; }

    public SendifyHttpException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class SendifyRequestException : SendifyException
{
    public SendifyRequestException(string? message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
