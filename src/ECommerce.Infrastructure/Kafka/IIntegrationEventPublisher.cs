namespace ECommerce.Infrastructure.Kafka;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(string topic, string key, string payload, CancellationToken cancellationToken = default);
}
