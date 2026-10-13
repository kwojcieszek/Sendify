using Microsoft.EntityFrameworkCore;
using Sendify.Data;
using Sendify.DataManager;

namespace Sendify.Repositories;

public class MessageRepository : IMessageRepository
{
    public async Task<IEnumerable<Message>> GetMessagesByUserIdAsync(string userId)
    {
        await using var db = new DataContext();
        
        return await db.Messages
            .Where(m => m.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(string groupId)
    {
        await using var db = new DataContext();
        
        return await db.Messages
            .Where(m => m.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageByIdAsync(string id, string userId)
    {
        await using var db = new DataContext();
        
        return await db.Messages
            .Where(m => m.UserId == userId && m.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        await using var db = new DataContext();
        
        await db.Messages.AddAsync(message);
        await db.SaveChangesAsync();
        
        return message;
    }
}
