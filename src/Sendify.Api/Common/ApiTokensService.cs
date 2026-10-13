using Microsoft.IdentityModel.Tokens;
using Sendify.Repositories;
using Sendify.Data;
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
    private readonly ITokenRepository _tokenRepository;

    public ApiTokensService(string jwtValidIssuer, string jwtValidAudience, string jwtSecret, ITokenRepository tokenRepository, int tokensUpdateIntervalSeconds = 60)
    {
        _jwtValidIssuer = jwtValidIssuer ?? throw new ArgumentNullException(nameof(jwtValidIssuer));
        _jwtValidAudience = jwtValidAudience ?? throw new ArgumentNullException(nameof(jwtValidAudience));
        _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
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

        await _tokenRepository.AddTokenAsync(token);
    }

    public async Task RevokeToken(string id, string userId)
    {
        await _tokenRepository.RevokeTokenAsync(id, userId);

        var token = await _tokenRepository.GetTokenByIdAsync(id, userId);

        if (token != null && token.IsRevoked)
        {
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
            List<string>? revokedTokens;

            if (_tokensUpdated == null)
            {
                _tokensUpdated = DateTime.UtcNow;

                revokedTokens = _tokenRepository.GetRevokedTokenHashesAsync().GetAwaiter().GetResult();
            }
            else
            {
                _tokensUpdated = DateTime.UtcNow;

                revokedTokens = _tokenRepository.GetRevokedTokenHashesAsync(_tokensUpdated).GetAwaiter().GetResult();
            }

            lock (_revokedTokens)
            {
                revokedTokens?.ToList().ForEach(t => _revokedTokens.Add(t));
            }
        }
    }
}