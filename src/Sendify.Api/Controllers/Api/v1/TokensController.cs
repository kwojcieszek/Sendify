using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Repositories;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.Shared;
using System.IdentityModel.Tokens.Jwt;
using Sendify.Api.Models;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiAuthorize]
public class TokensController : ControllerBase
{
    private readonly ILogger<TokensController> _logger;
    private readonly ITokensService _tokensService;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;

    public TokensController(ILogger<TokensController> logger, ITokensService tokensService, ITokenRepository tokenRepository, IUserRepository userRepository)
    {
        _logger = logger;
        _tokensService = tokensService;
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
    }

    [HttpGet()]
    public async Task<IEnumerable<Token>> GetTokens()
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        return await _tokenRepository.GetTokensByUserIdAsync(User.UserId()!);
    }

    [HttpPost()]
    public async Task<IActionResult> PostCreateNewToken(TokenModel newToken)
    {
        var user = await _userRepository.GetUserByIdAsync(User.UserId()!);

        if (user == null)
        {
            return Unauthorized();
        }

        var securityToken = _tokensService.CreateSecurityToken(user, newToken.ValidFrom ?? DateTime.UtcNow, newToken.ValidTo);

        await _tokensService.AddSecurityTokenToDatabase(securityToken, user, newToken.TokenName, newToken.Description);

        return await Task.FromResult<IActionResult>(Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(securityToken),
            expiration = securityToken.ValidTo
        }));
    }

    [HttpGet("{id}")]
    public async Task<Token?> GetTokenById(string id)
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return null;
        }

        return await _tokenRepository.GetTokenByIdAsync(id, User.UserId()!);
    }


    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> PostRevokeToken(string id)
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return await Task.FromResult<IActionResult>(Forbid());
        }

        await _tokenRepository.RevokeTokenAsync(id, User.UserId()!);

        return await Task.FromResult<IActionResult>(Ok());
    }
}