using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.Api.Controllers.Api.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthenticateController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost(template: "login")]
    public Task<IActionResult> PostLogin([FromBody] LoginModel model)
    {
        var authentication = new Authentication(new PasswordSha256());

        if (authentication.Auth(model.UserName, model.Password))
        {
            return Task.FromResult<IActionResult>(Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(authentication.SecurityToken),
                expiration = authentication.SecurityToken.ValidTo
            }));
        }

        return Task.FromResult<IActionResult>(Unauthorized());
    }

    [Authorize]
    [HttpGet(template: "user")]
    public async Task<User?> GetUser()
    {
        var db = new DataContext();
        return await db.Users.FirstOrDefaultAsync(u => u.Id == User.UserId());
    }
}