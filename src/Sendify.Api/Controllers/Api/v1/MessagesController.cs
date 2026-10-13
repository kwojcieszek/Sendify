using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sendify.Api.Common;
using Sendify.Api.Extensions;
using Sendify.Api.Models;
using Sendify.Repositories;
using Sendify.Data;
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
    private readonly IMessageRepository _messageRepository;

    public MessagesController(ILogger<MessagesController> logger, IFilter filter, IMessageRepository messageRepository)
    {
        _logger = logger;
        _filter = filter;
        _messageRepository = messageRepository;
    }

    [HttpGet()]
    public async Task<IEnumerable<Message>> GetMessages()
    {
        if (string.IsNullOrEmpty(User.UserId()))
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return [];
        }

        return await _messageRepository.GetMessagesByUserIdAsync(User.UserId()!);
    }

    [HttpGet("{id}")]
    public async Task<Message?> GetMessageById(string id)
    {
        if (User.UserId() == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;

            return null;
        }

        return await _messageRepository.GetMessageByIdAsync(id, User.UserId()!);
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

            await _messageRepository.AddMessageAsync(validMessage);

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