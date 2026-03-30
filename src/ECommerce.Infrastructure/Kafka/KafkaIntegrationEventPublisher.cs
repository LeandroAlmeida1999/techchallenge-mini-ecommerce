using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Kafka;

public sealed class KafkaIntegrationEventPublisher(
    IOptions<KafkaOptions> options,
    ILogger<KafkaIntegrationEventPublisher> logger) : IIntegrationEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        options.Value.ToProducerConfig()).Build();

    public Task PublishAsync(string topic, string key, string payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Publishing integration event to Kafka topic '{Topic}' with key '{Key}'.", topic, key);

        return _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = payload
        }, cancellationToken);
    }
    #region Dispose
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
    #endregion
}
