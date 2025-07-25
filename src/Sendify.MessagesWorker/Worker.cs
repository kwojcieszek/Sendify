using Sendify.MessageManagerSmsDigiWr21;
using Sendify.MessageService;

namespace Sendify.MessagesWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IMessageSender _messageSender;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        //var wr21Service = new Wr21Service(new TerminalTcp("192.168.1.1"), "username", "password");
        var wr21Service = new Wr21Service(new TerminalSerialPort("COM1"), "radwag", "radwag");

        _messageSender = new MessageSenderSmsDigiWr21(wr21Service,logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            _messageSender.SendMessage(new Message
            {
               Recipients = ["48602174021"],
               Body = "Test message from Sendify.MessagesWorker"
            });

            await Task.Delay(25000, stoppingToken);
        }
    }
}
