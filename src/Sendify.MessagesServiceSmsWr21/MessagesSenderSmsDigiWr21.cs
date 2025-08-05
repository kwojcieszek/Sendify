using Microsoft.Extensions.Logging;
using Sendify.Data;
using Sendify.MessagesService;

namespace Sendify.MessagesServiceSmsDigiWr21;

public class MessagesSenderSmsDigiWr21 : IMessagesSender
{
    private readonly ILogger _logger;
    private readonly Wr21Service _wr21Service;
    public MessageType ServiceType => MessageType.Sms;

    public MessagesSenderSmsDigiWr21()
    {
        _wr21Service = new Wr21Service(new TerminalSerialPort("COM3"), string.Empty, string.Empty, false);
        //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        var result = await _wr21Service.SendSms(message.Recipients.FirstOrDefault() ?? string.Empty, message.Body);

        return new ResultMessage(result, string.Empty);
    }
}