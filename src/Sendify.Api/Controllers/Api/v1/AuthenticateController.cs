using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Repositories;
using Sendify.Data;

namespace Sendify.Api.Controllers.Api.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuthentication _authentication;
    private readonly IUserRepository _userRepository;

    public AuthenticateController(IConfiguration configuration, IAuthentication authentication, IUserRepository userRepository)
    {
        _configuration = configuration;
        _authentication = authentication;
        _userRepository = userRepository;
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
        return await _userRepository.GetUserByIdAsync(User.UserId()!);
    }
}