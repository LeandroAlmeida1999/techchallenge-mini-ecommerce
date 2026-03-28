namespace ECommerce.Worker.Services;

public sealed class OutboxPublisherWorker(ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Polling pending outbox messages.");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
