using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sendify.Api.Models;
using Sendify.Data;
using Sendify.DataManager;

namespace Sendify.Api.Controllers.Api.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;

    public MessageController(ILogger<MessageController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<Message>> Get()
    {
        var db = new DataContext();

        return await db.Messages.ToArrayAsync();
    }

    [HttpPost]
    public async Task Post(MessageToSend message)
    {
        var db = new DataContext();

        try
        {
            if (string.IsNullOrEmpty(message.Body) || message.Recipients == null || !message.Recipients.Any())
            {
                Response.StatusCode = 400;
                return;
            }

            var validMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                Subject = message.Subject,
                Body = message.Body,
                Recipients = message.Recipients,
                Sender = message.Sender,
                MessageType = message.MessageType,
                IsSeparate = message.IsSeparate,
                Attachments = message.Attachments
            };

            await db.Messages.AddAsync(validMessage);

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message: {Message}", ex.Message);

            Response.StatusCode = 406;
        }
    }
}
