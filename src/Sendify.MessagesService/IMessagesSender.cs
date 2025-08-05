using Sendify.Data;

namespace Sendify.MessagesService;

public interface IMessagesSender
{
    public MessageType ServiceType { get; }
    public ResultMessage SendMessage(Message message);
    public Task<ResultMessage> SendMessageAsync(Message message);
}