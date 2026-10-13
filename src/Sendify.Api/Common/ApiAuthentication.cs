using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Sendify.Repositories;
using Sendify.Shared;

namespace Sendify.Api.Common;

public class ApiAuthentication : IAuthentication
{
    private readonly IPasswordService _passwordService;
    private readonly ITokensService _tokensService;
    private readonly IUserRepository _userRepository;

    public ApiAuthentication(IPasswordService passwordService, ITokensService tokensService, IUserRepository userRepository)
    {
        _passwordService = passwordService;
        _tokensService = tokensService;
        _userRepository = userRepository;
    }

    public JwtSecurityToken? Auth(string userName, string password, int expiresMinutes = 480)
    {
        var user = _userRepository.GetUserByUserNameAsync(userName).GetAwaiter().GetResult();

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