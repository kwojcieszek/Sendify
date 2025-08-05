using Sendify.MessageService;
using Sendify.MessageServiceSmsDigiWr21;
using Sendify.MessageServiceEmailSmtp;
using Sendify.MessageServiceSmsHttp;
using Sendify.ServiceCollection.Extensions;

namespace Sendify.MessagesWorker;

internal static class ServicesSettings
{
    public static void SetServices(IServiceCollection services)
    {
        services.SetDatabaseSettings();

        //services.AddSingleton<Wr21Service>(provider => new Wr21Service(new TerminalSerialPort("COM3"), string.Empty, string.Empty, false));

        //services.AddSingleton<IMessageSender, MessageSenderSmsDigiWr21>();

        services.AddSingleton<IMessageSender, MessageSenderSmsHttp>(provider=> new MessageSenderSmsHttp("http://10.10.254.250"));

        services.AddSingleton<IMessageSender, MessageSenderEmailSmtp>();

        services.AddSingleton<SenderService>();
    }
}