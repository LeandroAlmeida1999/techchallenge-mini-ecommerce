using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Kafka;

public sealed class KafkaIntegrationEventPublisher(
    IOptions<KafkaOptions> options,
    ILogger<KafkaIntegrationEventPublisher> logger) : IIntegrationEventPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer = new ProducerBuilder<Null, string>(new ProducerConfig
    {
        BootstrapServers = options.Value.BootstrapServers
    }).Build();

    public Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Publishing integration event to Kafka topic '{Topic}'.", topic);

        return _producer.ProduceAsync(topic, new Message<Null, string> { Value = payload }, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
