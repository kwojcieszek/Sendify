using Sendify.Shared.Extensions;

namespace Sendify.MessagesServiceSmsDigiWr21;

public class Wr21Service
{
    private readonly Wr21StreamService _streamService;
    private readonly SimplyLogger _logger = new SimplyLogger("ModemLogs");
    private readonly string _userName;
    private readonly string _password;
    private readonly bool _login;
    private readonly SmsMode _smsMode;
    private bool _isloggedIn = false;

    public Wr21Service(IStream stream, string userName, string password, bool login, SmsMode smsMode = SmsMode.Text)
    {
        _streamService = new Wr21StreamService(stream);
        _userName = userName;
        _password = password;
        _login = login;
        _smsMode = smsMode;
    }

    public async Task<bool> SendSmsAsync(string phone, string message)
    {
        var position = 0;
        var messageLength = _smsMode == SmsMode.Pdu 
            ? PduEncoder.GetMaxMessageLength(message) 
            : 160;
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

        if (_smsMode == SmsMode.Pdu)
        {
            return ExecuteSmsPdu(phone, message, timeout);
        }
        else
        {
            return ExecuteSmsText(phone, message, timeout);
        }
    }

    private bool ExecuteSmsText(string phone, string message, int timeout)
    {
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

    private bool ExecuteSmsPdu(string phone, string message, int timeout)
    {
        try
        {
            // Set PDU mode
            _streamService.WriteLine("AT+CMGF=0");

            var startDate = DateTime.Now;
            var line = string.Empty;

            // Wait for OK response
            while (true)
            {
                var newline = _streamService.ReadLineOrColonOrGreater();

                if (!string.IsNullOrEmpty(newline))
                {
                    _logger.Log(newline);
                }

                line += newline;

                if (line.Contains("OK"))
                {
                    break;
                }

                if (newline.RemoveLineEndings().Equals("ERROR"))
                {
                    _logger.Log("Error setting PDU mode");
                    return false;
                }

                if ((DateTime.Now - startDate).TotalMilliseconds > timeout)
                {
                    _logger.Log("Timeout setting PDU mode");
                    return false;
                }
            }

            // Encode message to PDU
            string pdu = PduEncoder.EncodePdu(phone, message,out int pduLength);

            _logger.Log($"PDU: {pdu}, Length: {pduLength}");

            // Send CMGS command with PDU length
            _streamService.WriteLine($"AT+CMGS={pduLength}");

            startDate = DateTime.Now;
            line = string.Empty;

            // Wait for > prompt
            while (true)
            {
                var newline = _streamService.ReadLineOrColonOrGreater();

                if (!string.IsNullOrEmpty(newline))
                {
                    _logger.Log(newline);
                }

                line += newline;

                if (newline.Contains(">"))
                {
                    break;
                }

                if (newline.RemoveLineEndings().Equals("ERROR"))
                {
                    _logger.Log("Error in CMGS command");
                    return false;
                }

                if ((DateTime.Now - startDate).TotalMilliseconds > timeout)
                {
                    _logger.Log("Timeout waiting for > prompt");
                    return false;
                }
            }

            // Send PDU followed by Ctrl+Z
            _streamService.WriteLine(pdu + "\x1A");

            startDate = DateTime.Now;
            line = string.Empty;

            // Wait for +CMGS response or OK
            while (true)
            {
                var newline = _streamService.ReadLineOrColonOrGreater();

                if (!string.IsNullOrEmpty(newline))
                {
                    _logger.Log(newline);
                }

                line += newline;

                if (line.Contains("+CMGS:") || line.Contains("OK"))
                {
                    _logger.Log("SMS sent successfully in PDU mode");
                    return true;
                }

                if (newline.RemoveLineEndings().Equals("ERROR") || line.Contains("CMS ERROR"))
                {
                    _logger.Log("Error sending SMS in PDU mode");
                    return false;
                }

                if ((DateTime.Now - startDate).TotalMilliseconds > timeout)
                {
                    _logger.Log("Timeout sending SMS in PDU mode");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Log($"Exception in PDU mode: {ex.Message}");
            return false;
        }
    }

    private bool Login()
    {
        return true;
    }
}