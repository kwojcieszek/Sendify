using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.Api.Common;

public class Authentication
{
    private readonly IPasswordService _passwordService;
    public static string? JwtValidIssuer { get; set; }
    public static string? JwtValidAudience { get; set; }
    public static string? JwtSecret { get; set; }
    public JwtSecurityToken SecurityToken { get; private set; }

    public Authentication(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    public bool Auth(string userName, string password, int expiresMinutes = 480)
    {
        var db = new DataContext();
        
        var user = db.Users.FirstOrDefault(u => u.UserName == userName);

        if(user == null)
        {
            return false;
        }

        if(_passwordService == null)
        {
            throw new NoNullAllowedException("IPasswordService is not acceptable for method Auth");
        }

        if (_passwordService.ComparePassword(password, user.Password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.PrimarySid, user.Id),
                new Claim(ClaimTypes.PrimaryGroupSid, user.GroupId!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            if (user.PhoneNumber != null)
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            if(JwtSecret == null || JwtValidIssuer == null || JwtValidAudience == null)
            {
                throw new InvalidOperationException("JWT configuration is not set. Please set JwtSecret, JwtValidIssuer, and JwtValidAudience in the configuration.");
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));

            SecurityToken = new JwtSecurityToken(
                issuer: JwtValidIssuer,
                audience: JwtValidAudience,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(expiresMinutes),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return true;
        }

        return false;
    }

    public void Revoke()
    {
        var securityToken = new JwtSecurityToken();
    }
}