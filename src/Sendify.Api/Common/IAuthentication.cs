using System.IdentityModel.Tokens.Jwt;

namespace Sendify.Api.Common;

public interface IAuthentication
{
    JwtSecurityToken? Auth(string userName, string password, int expiresMinutes = 480);
}