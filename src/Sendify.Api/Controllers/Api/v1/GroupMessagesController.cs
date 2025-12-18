using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiAuthorize]
public class GroupMessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;

    public GroupMessagesController(ILogger<MessagesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<Message>> GetMessagesByGroup()
    {
        if (string.IsNullOrEmpty(User.UserId()) || string.IsNullOrEmpty(User.GroupId()))
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        await using var db = new DataContext();

        return await db.Messages
            .Where(m => m.GroupId == User.GroupId())
            .ToListAsync();
    }
}