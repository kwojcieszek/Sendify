using Sendify.Data;

namespace Sendify.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetMessagesByUserIdAsync(string userId);
    Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(string groupId);
    Task<Message?> GetMessageByIdAsync(string id, string userId);
    Task<Message> AddMessageAsync(Message message);
}
