using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.Shared;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;

    public MessageController(ILogger<MessageController> logger)
    {
        _logger = logger;
    }

    [HttpGet("messages_by_user")]
    public async Task<IEnumerable<Message>> GetByUser()
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        var db = new DataContext();

        return await db.Messages
            .Where(m=>m.UserId == User.UserId())
            .ToListAsync();
    }

    [HttpGet("messages_by_group")]
    public async Task<IEnumerable<Message>> GetByGroup()
    {
        if (User.GroupId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        var db = new DataContext();

        return await db.Messages
            .Where(m => m.GroupId == User.GroupId())
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<Message?> GetById(string id)
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return null;
        }

        var db = new DataContext();

        return await db.Messages
            .Where(m => m.UserId == User.UserId() && m.Id == id)
            .FirstOrDefaultAsync();
    }

    [HttpPost]
    public async Task<ReplyMessage?> Post(MessageToSend message)
    {
        var db = new DataContext();

        try
        {
            if(User.UserId() == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;
                return null;
            }

            if (string.IsNullOrEmpty(message.Body) || message.Recipients == null || !message.Recipients.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return null;
            }

            var validMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                UserId = User.UserId()!,
                GroupId = User.GroupId(),
                Subject = message.Subject,
                Body = message.Body,
                Recipients = message.Recipients,
                Sender = message.Sender,
                MessageType = message.MessageType,
                IsSeparate = message.IsSeparate,
                Attachments = message.Attachments,
                CreatedAt = DateTime.UtcNow
            };

            await db.Messages.AddAsync(validMessage);

            await db.SaveChangesAsync();

            return new ReplyMessage()
            {
                Id = validMessage.Id,
                CreatedAt = validMessage.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message: {Message}", ex.Message);

            Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
        }

        return null;
    }
}
