namespace Sendify.MessageManagerSmsDigiWr21;

public class Wr21Service
{
    private readonly Wr21StreamService _streamService;
    private readonly string _userName;
    private readonly string _password;
    private bool _isloggedIn = false;

    public Wr21Service(IStream stream, string userName, string password)
    {
        _streamService = new Wr21StreamService(stream);
        _userName = userName;
        _password = password;
    }

    public void SendSms(string phone,string message)
    {
        while(true) 
        {
            var line = _streamService.ReadLineOrColonOrGreater();

            switch(line)
            {
                case string l when l.Contains("Username:"):
                    _streamService.WriteLine(_userName);
                    break;
                case string l when l.Contains("Password:"):
                    _streamService.WriteLine(_password);
                    break;
                case string l when l.Contains("Welcome"):
                    _isloggedIn = true;
                    break;
                case string l when l.Contains("SMS send success"):
                    return;
            }

            if (_isloggedIn)
            {
                _streamService.WriteLine($"sendsms {phone} \"{message}\"");
            }
        }
    }
}
