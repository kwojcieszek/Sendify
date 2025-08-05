using Sendify.MessagesService;
using Sendify.MessagesServiceEmailSmtp;
using Sendify.MessagesServiceSmsHttp;
using Sendify.ServiceCollection.Extensions;

namespace Sendify.MessagesWorker;

internal static class ServicesSettings
{
    public static void SetServices(IServiceCollection services)
    {
        services.SetDatabaseSettings();

        //services.AddSingleton<Wr21Service>(provider => new Wr21Service(new TerminalSerialPort("COM3"), string.Empty, string.Empty, false));

        //services.AddSingleton<IMessageSender, MessageSenderSmsDigiWr21>();

        services.AddSingleton<IMessagesSender, MessagesSenderSmsHttp>(provider=> new MessagesSenderSmsHttp("http://10.10.254.250"));

        services.AddSingleton<IMessagesSender, MessagesSenderEmailSmtp>();

        services.AddSingleton<SenderService>();
    }
}