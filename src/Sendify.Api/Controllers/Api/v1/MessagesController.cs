using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.FilterService;
using Sendify.Shared;
using Sendify.Shared.Extensions;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiAuthorize]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly IFilter _filter;

    public MessagesController(ILogger<MessagesController> logger, IFilter filter)
    {
        _logger = logger;
        _filter = filter;
    }

    [HttpGet()]
    public async Task<IEnumerable<Message>> GetMessages()
    {
        if (string.IsNullOrEmpty(User.UserId()))
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        var db = new DataContext();

        return await db.Messages
            .Where(m=>m.UserId == User.UserId())
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<Message?> GetMessageById(string id)
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

    [HttpPost()]
    public async Task<ReplyMessageModel?> PostSendMessage(MessageToSendModel message)
    {
        try
        {
            if(User.UserId() == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;
                return null;
            }

            if (message == null || string.IsNullOrEmpty(message.Body) || message.Recipients == null || !message.Recipients.Any())
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return null;
            }

            var attachments = message.Attachments?.Select(a => new Attachment { Id = Guid.NewGuid().ToString(), FileName = a.FileName, ContentType = a.ContentType, Content = a.Content }).ToArray();

            var validMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                UserId = User.UserId()!,
                GroupId = User.GroupId(),
                Subject = message.Subject.JsonEscape(),
                Body = message.Body.JsonEscape(),
                Recipients = message.Recipients,
                Sender = message.Sender,
                MessageType = message.MessageType,
                IsSeparate = message.IsSeparate,
                Attachments = attachments,
                Priority = message?.Priority ?? 5,
                CreatedAt = DateTime.UtcNow
            };

            var filterResult = _filter.IsMessageAllowed(validMessage);

            if(!filterResult.IsAllowed)
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                
                await Response.WriteAsync(filterResult.Reason);

                return null;
            }

            var db = new DataContext();

            await db.Messages.AddAsync(validMessage);

            await db.SaveChangesAsync();

            return new ReplyMessageModel()
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