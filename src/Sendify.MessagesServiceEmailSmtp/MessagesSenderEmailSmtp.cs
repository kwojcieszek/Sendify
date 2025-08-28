using FluentEmail.Smtp;
using Microsoft.Extensions.Logging;
using Sendify.Data;
using Sendify.MessagesService;
using System.Net.Mail;
using System.Net;
using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace Sendify.MessagesServiceEmailSmtp;

public class MessagesSenderEmailSmtp : IMessagesSender
{
    private readonly ILogger<MessagesSenderEmailSmtp> _logger;
    private readonly SmtpSender _smtpSender;
    public MessageType ServiceType => MessageType.Email;
    public string Sender { get; init; }

    public MessagesSenderEmailSmtp(ILogger<MessagesSenderEmailSmtp> logger, string host, string sender, int port,
        bool enableSsl, string userName, string password)
    {
        Sender = sender;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _smtpSender = CreateSmtpSenders(host,sender,port,enableSsl,userName,password);
    }

    public ResultMessage SendMessage(Message message)
    {
        return SendMessageAsync(message).Result;
    }

    public async Task<ResultMessage> SendMessageAsync(Message message)
    {
        if(message.Recipients == null)
        {
            return new ResultMessage(false, "No recipients specified");
        }

        var attachments = new List<FluentEmail.Core.Models.Attachment>();

        foreach (var att in message.Attachments ?? Enumerable.Empty<Data.Attachment>())
        {
            if (string.IsNullOrEmpty(att.Content))
            {
                _logger.LogWarning("Attachment {AttachmentId} has no content", att.Id);
                continue;
            }

            var bytes = Convert.FromBase64String(att.Content);

            attachments.Add(new FluentEmail.Core.Models.Attachment()
            {
                ContentType = att.ContentType ?? "application/octet-stream",
                Filename = att.FileName,
                Data = new MemoryStream(bytes)
            });
        }
        
        var email = Email
            .From(message.Sender)
            .To(message.Recipients.Select(r => new Address(r)))
            .Subject(message.Subject)
            .Attach(attachments)
            .Body(message.Body);
        
        var result = await _smtpSender.SendAsync(email);

        return new ResultMessage(result.Successful, string.Join("\n", result.ErrorMessages));
    }

    private SmtpSender CreateSmtpSenders(string host, string sender, int port,bool enableSsl, string userName, string password)
    {
        var smtpSender = new SmtpSender(() => new SmtpClient(host)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Port = port,
            Credentials = new NetworkCredential(userName, password)
        });

        return smtpSender;
    }
}