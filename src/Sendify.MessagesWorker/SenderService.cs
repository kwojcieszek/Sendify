using Microsoft.EntityFrameworkCore;
using Sendify.Data;
using Sendify.DataManager;
using Sendify.MessagesService;

namespace Sendify.MessagesWorker;

public class SenderService
{
    private int SendingAttempts { get; set; } = 5;
    private readonly ILogger<Worker> _logger;
    private readonly IEnumerable<IMessagesSender> _messageSender;

    public SenderService(ILogger<Worker> logger, IEnumerable<IMessagesSender> messageSender)
    {
        _logger = logger;
        _messageSender = messageSender;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        var messageTypes = Enum.GetValues(typeof(MessageType)).Cast<MessageType>();

        foreach (var messageType in messageTypes)
        {
            var messageSender = _messageSender.FirstOrDefault(t => t.ServiceType == messageType);

            if (messageSender == null)
            {
                throw new InvalidOperationException($"No message sender found for message type: {messageType}.");
            }

            var task = TasksManagement(messageType, messageSender, new DataContext(), cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task TasksManagement(MessageType messageType, IMessagesSender messageSender,DataContext dataContext, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await MessageProcessing(messageType, messageSender, dataContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing messages of type {MessageType}: {Message}", messageType, ex.Message);
            }

            await Task.Delay(500, cancellationToken);
        }
    }

    private async Task MessageProcessing(MessageType messageType, IMessagesSender messageSender, DataContext dataContext)
    {
        
        var messages = await dataContext.Messages
                .Where(t => t.MessageType == messageType &&
                (t.SendingStatus == SendingStatus.None || (t.SendingStatus == SendingStatus.Failed && t.SendingAttempts < SendingAttempts)))
                .ToArrayAsync();

        foreach (var message in messages)
        {
            message.SendingAttempts++;
            message.SendingStatus = SendingStatus.Pending;
        }

        await dataContext.SaveChangesAsync();

        foreach (var message in messages)
        {
            var result = await messageSender.SendMessageAsync(message);

            if (result.IsSuccess)
            {
                message.SendingStatus = SendingStatus.Sent;
                message.SentAt = DateTime.UtcNow;
            }
            else
            {
                message.SendingStatus = SendingStatus.Failed;
                message.FailedAt = DateTime.UtcNow;
                message.ErrorMessage = result.ErrorMessage;
            }          

            await dataContext.SaveChangesAsync();
        }
    }
}