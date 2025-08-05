using Sendify.JobsWorker.Jobs;

namespace Sendify.JobsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var usersJobs = new UsersJobs();
            usersJobs.CheckUsersPasswords();

            await Task.Delay(60000, stoppingToken);
        }
    }
}
