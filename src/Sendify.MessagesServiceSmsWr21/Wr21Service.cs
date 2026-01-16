using Sendify.Shared.Extensions;

namespace Sendify.MessagesServiceSmsDigiWr21;

public class Wr21Service
{
    private readonly Wr21StreamService _streamService;
    private readonly SimplyLogger _logger = new SimplyLogger("ModemLogs");
    private readonly string _userName;
    private readonly string _password;
    private readonly bool _login;
    private bool _isloggedIn = false;

    public Wr21Service(IStream stream, string userName, string password, bool login)
    {
        _streamService = new Wr21StreamService(stream);
        _userName = userName;
        _password = password;
        _login = login;
    }

    public async Task<bool> SendSms(string phone, string message)
    {
        var position = 0;
        var messageLength = 160;
        var result = false;

        do
        {
            var text = message.Substring(position, messageLength + position > message.Length ? message.Length - position : messageLength);

            result = ExecuteSms(phone, text, _login);

            await Task.Delay(500);

            position += messageLength;

        } while (position < message.Length);

        return result;
    }

    private bool ExecuteSms(string phone, string message, bool login)
    {
        int timeout = 60_000;

        if (login)
        {
            _isloggedIn = Login();

            if (!_isloggedIn)
            {
                return false;
            }
        }

        _streamService.WriteLine($"sendsms {phone} \"{message.ReplacePolishDiacritics().NormalizeText()}\"");

        var startDate = DateTime.Now;

        var line = string.Empty;

        while (true)
        {
            var newline = _streamService.ReadLineOrColonOrGreater();

            if(!string.IsNullOrEmpty(newline))
            {
                _logger.Log(newline);
            }

            line += newline;

            if (line.Contains("SMS send success"))
            {
                return true;
            }

            if (line.Contains("SMS send failure") || newline.RemoveLineEndings().Equals("ERROR"))
            {
                return false;
            }

            if ((DateTime.Now - startDate).TotalMilliseconds > timeout)
            {
                return false;
            }
        }
    }

    private bool Login()
    {
        return true;
    }
}