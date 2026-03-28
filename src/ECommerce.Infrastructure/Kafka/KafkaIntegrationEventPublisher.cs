using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Kafka;

public sealed class KafkaIntegrationEventPublisher(
    IOptions<KafkaOptions> options,
    ILogger<KafkaIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        logger.LogInformation("Publishing integration event to Kafka topic '{Topic}'.", topic);

        return producer.ProduceAsync(topic, new Message<Null, string> { Value = payload }, cancellationToken);
    }
}
