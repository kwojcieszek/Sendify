using Microsoft.Extensions.Logging;
using Sendify.MessageService;

namespace Sendify.MessageManagerSmsDigiWr21;

public class MessageSenderSmsDigiWr21 : IMessageSender
{
    private readonly ILogger _logger;
    private readonly Wr21Service _wr21Service;

    public MessageSenderSmsDigiWr21(Wr21Service wr21Service, ILogger logger)
    {
        _wr21Service = wr21Service;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool SendMessage(Message message)
    {
        return SendMessageAsync(message).GetAwaiter().GetResult();
    }

    public async Task<bool> SendMessageAsync(Message message)
    {
        bool result = false;

        await Task.Run(() =>
        {
            try
            {
                _wr21Service.SendSms(message.Recipients.FirstOrDefault() ?? string.Empty, message.Body);

                result = true;
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Error sending message: {Message}", e.Message);
            }
        });

        return result;
    }
}