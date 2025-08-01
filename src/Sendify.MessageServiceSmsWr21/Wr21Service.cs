using System.Reflection;

namespace Sendify.MessageServiceSmsDigiWr21;

public class Wr21Service
{
    private readonly Wr21StreamService _streamService;
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
        int timeout = 5000;
        DateTime startDate;

        if (login)
        {
            _isloggedIn = Login();

            if (!_isloggedIn)
            {
                return false;
            }
        }

        _streamService.WriteLine($"sendsms {phone} \"{message}\"");

        startDate = DateTime.Now;

        while (true)
        {
            var line = _streamService.ReadLineOrColonOrGreater();

            if (line != null && line.Contains("SMS send success"))
            {
                return true;
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
