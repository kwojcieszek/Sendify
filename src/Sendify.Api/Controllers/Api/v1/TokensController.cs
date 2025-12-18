using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
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

    public TokensController(ILogger<TokensController> logger, ITokensService tokensService)
    {
        _logger = logger;
        _tokensService = tokensService;
    }

    [HttpGet()]
    public async Task<IEnumerable<Token>> GetTokens()
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        await using var db = new DataContext();

        return await db.Tokens
            .Where(m => m.UserId == User.UserId())
            .ToListAsync();
    }

    [HttpPost()]
    public async Task<IActionResult> PostCreateNewToken(NewTokenModel newToken)
    {
        await using var db = new DataContext();

        var user = db.Users.FirstOrDefault(u => u.Id == User.UserId());

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

        await using var db = new DataContext();

        return await db.Tokens
            .Where(t => t.UserId == User.UserId() && t.Id == id)
            .FirstOrDefaultAsync();
    }


    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> PostRevokeToken(string id)
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return await Task.FromResult<IActionResult>(Forbid());
        }

        await _tokensService.RevokeToken(id, User.UserId()!);

        return await Task.FromResult<IActionResult>(Ok());
    }
}