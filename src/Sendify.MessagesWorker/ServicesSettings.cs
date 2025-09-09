using Sendify.MessagesService;
using Sendify.MessagesServiceEmailSmtp;
using Sendify.MessagesServiceSmsDigiWr21;
using Sendify.ServiceCollection.Extensions;

namespace Sendify.MessagesWorker;

internal static class ServicesSettings
{
    public static void SetServices(IServiceCollection services)
    {
        services.SetDatabaseSettings();

        services.AddSingleton<Wr21Service>(provider => new Wr21Service(new TerminalSerialPort(Settings.TerminalSerialPortSettings.Instance.PortName), string.Empty, string.Empty, false));
        
        services.AddSingleton<IMessagesSender, MessagesSenderSmsDigiWr21>();

        services.AddSingleton<SenderService>();

        Settings.SmtpSettings.Instance.SmtpClientList.ForEach(smtpClientData =>
            {
                services.AddSingleton<IMessagesSender>(sp=>
                    ActivatorUtilities.CreateInstance<MessagesSenderEmailSmtp>(
                        sp,
                        smtpClientData.Host,
                        smtpClientData.Sender,
                        smtpClientData.Port,
                        smtpClientData.EnableSsl,
                        smtpClientData.UserName,
                        smtpClientData.Password
                    )
                );
            }
        );
    }
}