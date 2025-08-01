using Microsoft.Extensions.Logging;
using Sendify.MessageService;
using Sendify.Data;

namespace Sendify.MessageServiceEmailSmtp;

public class MessageSenderEmailSmtp : IMessageSender
{
    private readonly ILogger _logger;

    public MessageType ServiceType => MessageType.Email;

    public MessageSenderEmailSmtp()
    {
        //  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ResultMessage SendMessage(Message message)
    {
        return new ResultMessage(false, "");
    }

    public Task<ResultMessage> SendMessageAsync(Message message)
    {
        return Task.FromResult(new ResultMessage(false, ""));
    }
}