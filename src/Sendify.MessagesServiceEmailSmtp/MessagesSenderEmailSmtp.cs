using Microsoft.Extensions.Logging;
using Sendify.Data;
using Sendify.MessagesService;

namespace Sendify.MessagesServiceEmailSmtp;

public class MessagesSenderEmailSmtp : IMessagesSender
{
    private readonly ILogger _logger;

    public MessageType ServiceType => MessageType.Email;

    public MessagesSenderEmailSmtp()
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