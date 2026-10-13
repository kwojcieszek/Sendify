using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Repositories;
using Sendify.Data;
using Sendify.Shared;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiAuthorize]
public class GroupMessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly IMessageRepository _messageRepository;

    public GroupMessagesController(ILogger<MessagesController> logger, IMessageRepository messageRepository)
    {
        _logger = logger;
        _messageRepository = messageRepository;
    }

    [HttpGet]
    public async Task<IEnumerable<Message>> GetMessagesByGroup()
    {
        if (string.IsNullOrEmpty(User.UserId()) || string.IsNullOrEmpty(User.GroupId()))
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        return await _messageRepository.GetMessagesByGroupIdAsync(User.GroupId()!);
    }
}