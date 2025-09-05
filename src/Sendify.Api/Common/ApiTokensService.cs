using Microsoft.IdentityModel.Tokens;
using Sendify.Data;
using Sendify.DataManager;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sendify.Api.Common;

public class ApiTokensService : ITokensService
{
    private string _jwtValidIssuer;
    private string _jwtValidAudience;
    private string _jwtSecret;
    private HashSet<string> _revokedTokens = new();
    private DateTime? _tokensUpdated;
    private int _tokensUpdateIntervalSeconds;

    public ApiTokensService(string jwtValidIssuer, string jwtValidAudience, string jwtSecret, int tokensUpdateIntervalSeconds = 60)
    {
        _jwtValidIssuer = jwtValidIssuer ?? throw new ArgumentNullException(nameof(jwtValidIssuer));
        _jwtValidAudience = jwtValidAudience ?? throw new ArgumentNullException(nameof(jwtValidAudience));
        _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        _tokensUpdateIntervalSeconds = tokensUpdateIntervalSeconds;
    }

    public string GetTokenHash(string token)
    {
        using SHA256 sha256Hash = SHA256.Create();

        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(token));

        var builder = new StringBuilder();

        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    public JwtSecurityToken CreateSecurityToken(User user, DateTime validFrom, DateTime validTo)
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

        if (_jwtSecret == null || _jwtValidIssuer == null || _jwtValidAudience == null)
        {
            throw new InvalidOperationException("JWT configuration is not set. Please set JwtSecret, JwtValidIssuer, and JwtValidAudience in the configuration.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

        var securityToken = new JwtSecurityToken(
            issuer: _jwtValidIssuer,
            audience: _jwtValidAudience,
            notBefore: DateTime.UtcNow,
            expires: validTo,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return securityToken;
    }

    public async Task AddSecurityTokenToDatabase(JwtSecurityToken jwtSecurityToken, User user, string? tokenName = null, string? description = null)
    {
        var db = new DataContext();

        var securityTokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        var token = new Token()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            TokenName = tokenName,
            Description = description,
            ValidFrom = jwtSecurityToken.ValidFrom,
            ValidTo = jwtSecurityToken.ValidTo,
            TokenHash = GetTokenHash(securityTokenString)
        };

        db.Tokens.Add(token);

        await db.SaveChangesAsync();
    }
    
    public async Task RevokeToken(string id,string userId)
    {
       var db = new DataContext();

        var token = db.Tokens.FirstOrDefault(t => t.UserId == userId && t.Id == id);

        if (token != null && !token.IsRevoked)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            lock (_revokedTokens)
            {
                _revokedTokens.Add(token.TokenHash);
            }
        }
    }

    public bool IsTokenRevoked(string token)
    {
        UpdatedTokens();

        lock (_revokedTokens)
        {
            return _revokedTokens.Contains(GetTokenHash(token));
        }
    }

    private void UpdatedTokens()
    {
        if (_tokensUpdated == null || (DateTime.UtcNow - _tokensUpdated.Value).TotalSeconds > _tokensUpdateIntervalSeconds)
        {
            var db = new DataContext();
            List<string>? revokedTokens;
            
            if (_tokensUpdated == null)
            {
                _tokensUpdated = DateTime.UtcNow;

                revokedTokens = db.Tokens
                    .Where(t => t.IsRevoked && t.ValidTo > DateTime.UtcNow)
                    .Select(t => t.TokenHash).ToList();
            }
            else
            {
                _tokensUpdated = DateTime.UtcNow;

                revokedTokens = db.Tokens
                  .Where(t => t.IsRevoked && t.ValidTo > DateTime.UtcNow && t.RevokedAt > _tokensUpdated)
                  .Select(t => t.TokenHash).ToList(); ;
            }

            lock (_revokedTokens)
            {
                revokedTokens?.ToList().ForEach(t => _revokedTokens.Add(t));
            }
        }
    }
}