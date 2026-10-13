using Sendify.Data;

namespace Sendify.Repositories;

public interface ITokenRepository
{
    Task<IEnumerable<Token>> GetTokensByUserIdAsync(string userId);
    Task<Token?> GetTokenByIdAsync(string id, string userId);
    Task<Token> AddTokenAsync(Token token);
    Task RevokeTokenAsync(string id, string userId);
    Task<List<string>> GetRevokedTokenHashesAsync(DateTime? since = null);
}
