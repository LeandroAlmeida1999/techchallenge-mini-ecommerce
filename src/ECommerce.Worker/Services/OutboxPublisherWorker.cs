using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Kafka;
using Microsoft.Extensions.Options;

namespace ECommerce.Worker.Services;

public sealed class OutboxPublisherWorker(
    IOutboxRepository outboxRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingMessages = await outboxRepository.GetPendingAsync(20, stoppingToken);

            foreach (var message in pendingMessages)
            {
                try
                {
                    await integrationEventPublisher.PublishAsync(
                        kafkaOptions.Value.Topic,
                        message.Payload,
                        stoppingToken);

                    await outboxRepository.UpdateAsync(
                        message with
                        {
                            ProcessedOnUtc = DateTime.UtcNow,
                            Status = "Processed",
                            Error = null
                        },
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error publishing outbox message {OutboxMessageId}.", message.Id);

                    await outboxRepository.UpdateAsync(
                        message with
                        {
                            Status = "Failed",
                            Error = ex.Message
                        },
                        stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
