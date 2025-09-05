using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.Api.Common;

public class ApiAuthentication : IAuthentication
{
    private readonly IPasswordService _passwordService;
    private readonly ITokensService _tokensService;

    public ApiAuthentication(IPasswordService passwordService, ITokensService tokensService)
    {
        _passwordService = passwordService;
        _tokensService = tokensService;
    }

    public JwtSecurityToken? Auth(string userName, string password, int expiresMinutes = 480)
    {
        var db = new DataContext();
        
        var user = db.Users.FirstOrDefault(u => u.UserName == userName);

        if(user == null)
        {
            return null;
        }

        if(_passwordService == null)
        {
            throw new NoNullAllowedException("IPasswordService is not acceptable for method Auth");
        }

        if (_passwordService.ComparePassword(password, user.Password))
        {
            var jwtSecurityToken = _tokensService.CreateSecurityToken(user, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(expiresMinutes));

            _tokensService.AddSecurityTokenToDatabase(jwtSecurityToken, user);

            return jwtSecurityToken;
        }

        return null;
    }
}