namespace ECommerce.Infrastructure.Kafka;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default);
}
