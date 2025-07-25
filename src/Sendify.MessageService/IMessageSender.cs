namespace Sendify.MessageService;

public interface IMessageSender
{
    public bool SendMessage(Message message);
    public Task<bool> SendMessageAsync(Message message);
}