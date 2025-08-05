namespace Sendify.MessagesWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly SenderService _senderService;
    
    public Worker(ILogger<Worker> logger, SenderService senderService)
    {
        _logger = logger;
        _senderService = senderService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await _senderService.StartAsync(stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
