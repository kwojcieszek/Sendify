using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Data;
using Sendify.DataManager;

namespace Sendify.Api.Controllers.Api.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuthentication _authentication;

    public AuthenticateController(IConfiguration configuration, IAuthentication authentication)
    {
        _configuration = configuration;
        _authentication = authentication;
    }

    [HttpPost(template: "login")]
    public Task<IActionResult> PostLogin([FromBody] LoginModel model)
    {
        var securityToken = _authentication.Auth(model.UserName, model.Password);

        if (securityToken != null)
        {
            return Task.FromResult<IActionResult>(Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                expiration = securityToken.ValidTo
            }));
        }

        return Task.FromResult<IActionResult>(Unauthorized());
    }

    [Authorize]
    [HttpGet(template: "user")]
    public async Task<User?> GetUser()
    {
        await using var db = new DataContext();

        return await db.Users.FirstOrDefaultAsync(u => u.Id == User.UserId());
    }
}