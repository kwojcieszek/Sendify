using Sendify.MessageService;
using Sendify.MessageServiceSmsDigiWr21;
using Sendify.MessageServiceEmailSmtp;
using Sendify.MessagesWorker.Extensions;

namespace Sendify.MessagesWorker;

internal static class ServicesSettings
{
    public static void SetServices(IServiceCollection services)
    {
        services.AddDatacontex();

        //services.AddSingleton<Wr21Service>(provider => new Wr21Service(new TerminalSerialPort("COM3"), string.Empty, string.Empty, false));

        services.AddSingleton<IMessageSender, MessageSenderSmsDigiWr21>();

        services.AddSingleton<IMessageSender, MessageSenderEmailSmtp>();

        services.AddSingleton<SenderService>();
    }
}