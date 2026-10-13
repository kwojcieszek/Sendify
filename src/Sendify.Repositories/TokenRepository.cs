using Microsoft.EntityFrameworkCore;
using Sendify.Data;
using Sendify.DataManager;

namespace Sendify.Repositories;

public class TokenRepository : ITokenRepository
{
    public async Task<IEnumerable<Token>> GetTokensByUserIdAsync(string userId)
    {
        await using var db = new DataContext();
        
        return await db.Tokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<Token?> GetTokenByIdAsync(string id, string userId)
    {
        await using var db = new DataContext();
        
        return await db.Tokens
            .Where(t => t.UserId == userId && t.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Token> AddTokenAsync(Token token)
    {
        await using var db = new DataContext();
        
        await db.Tokens.AddAsync(token);
        await db.SaveChangesAsync();
        
        return token;
    }

    public async Task RevokeTokenAsync(string id, string userId)
    {
        await using var db = new DataContext();
        
        var token = await db.Tokens
            .Where(t => t.UserId == userId && t.Id == id)
            .FirstOrDefaultAsync();
        
        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetRevokedTokenHashesAsync(DateTime? since = null)
    {
        await using var db = new DataContext();
        
        var query = db.Tokens.Where(t => t.IsRevoked && t.ValidTo > DateTime.UtcNow);
        
        if (since.HasValue)
        {
            query = query.Where(t => t.RevokedAt > since.Value);
        }
        
        return await query.Select(t => t.TokenHash).ToListAsync();
    }
}
