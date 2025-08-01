using Sendify.Data;

namespace Sendify.MessageService;

public interface IMessageSender
{
    public MessageType ServiceType { get; }
    public ResultMessage SendMessage(Message message);
    public Task<ResultMessage> SendMessageAsync(Message message);
}