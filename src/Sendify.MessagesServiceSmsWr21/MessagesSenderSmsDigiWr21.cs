using Microsoft.Extensions.Logging;
using Sendify.Data;
using Sendify.MessagesService;

namespace Sendify.MessagesServiceSmsDigiWr21;

public class MessagesSenderSmsDigiWr21 : IMessagesSender
{
    private readonly ILogger<MessagesSenderSmsDigiWr21> _logger;
    private readonly Wr21Service _wr21Service;
    public MessageType ServiceType => MessageType.Sms;
    public string Sender => string.Empty;

    public MessagesSenderSmsDigiWr21(ILogger<MessagesSenderSmsDigiWr21> logger, Wr21Service wr21Service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _wr21Service = wr21Service ?? throw new ArgumentNullException(nameof(wr21Service)); ;
    }

    public ResultMessage SendMessage(Message message)
    {
        return SendMessageAsync(message).GetAwaiter().GetResult();
    }

    public async Task<ResultMessage> SendMessageAsync(Message message)
    {
        if (message?.Recipients == null)
        {
            return new ResultMessage(false, "Message or Recipients cannot be null.");
        }

        bool result = false;

        foreach (var recipient in message.Recipients)
        {
            result |= await _wr21Service.SendSmsAsync(recipient, message.Body);
        }

        return new ResultMessage(result, string.Empty);
    }
}