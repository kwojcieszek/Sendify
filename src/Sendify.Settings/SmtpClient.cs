namespace Sendify.Settings;

public class SmtpClient
{
    public string Host { get; set; }
    public string Sender { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
