using Sendify.MessageManager;

namespace Sendify.MessageManagerEmailSmtp;

public class MessageSenderEmailSmtp : IMessageSender
{
    public bool SendMessage(Message message)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendMessageAsync(Message message)
    {
        throw new NotImplementedException();
    }
}