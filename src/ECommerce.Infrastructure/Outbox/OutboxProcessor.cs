using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Outbox;

public sealed class OutboxProcessor(
    IOutboxRepository outboxRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    private const int BatchSize = 20;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var pendingMessages = await outboxRepository.GetPendingAsync(BatchSize, cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                await integrationEventPublisher.PublishAsync(
                    kafkaOptions.Value.Topic,
                    message.PartitionKey,
                    message.Payload,
                    cancellationToken);

                await outboxRepository.UpdateAsync(
                    message with
                    {
                        ProcessedOnUtc = DateTime.UtcNow,
                        Status = "Processed",
                        Error = null
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                var retriesAfterFailure = message.Retries + 1;
                var statusAfterFailure = retriesAfterFailure >= OutboxMessage.MaxRetries
                    ? "Exhausted"
                    : "Failed";

                logger.LogError(ex, "Error publishing outbox message {OutboxMessageId}.", message.Id);

                await outboxRepository.UpdateAsync(
                    message with
                    {
                        Status = statusAfterFailure,
                        Error = ex.Message
                    },
                    cancellationToken);
            }
        }
    }
}
