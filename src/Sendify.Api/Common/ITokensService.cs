using Sendify.Data;
using System.IdentityModel.Tokens.Jwt;

namespace Sendify.Api.Common;

public interface ITokensService
{
    string GetTokenHash(string token);

    JwtSecurityToken CreateSecurityToken(User user, DateTime validFrom, DateTime validTo);

    Task AddSecurityTokenToDatabase(JwtSecurityToken jwtSecurityToken, User user, string? tokenName = null, string? description = null);

    Task RevokeToken(string id,string userId);

    bool IsTokenRevoked(string token);
}