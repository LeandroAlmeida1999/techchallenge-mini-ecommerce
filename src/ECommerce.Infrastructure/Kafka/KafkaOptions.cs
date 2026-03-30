using Confluent.Kafka;

namespace ECommerce.Infrastructure.Kafka;

public sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string Topic { get; init; }
    public required string ClientId { get; init; }
    public string? SecurityProtocol { get; init; }
    public string? SaslMechanism { get; init; }
    public string? SaslUsername { get; init; }
    public string? SaslPassword { get; init; }
    public bool? EnableSslCertificateVerification { get; init; }

    public ProducerConfig ToProducerConfig()
    {
        if (string.IsNullOrWhiteSpace(BootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers deve ser configurado.");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("Kafka:ClientId deve ser configurado.");

        var config = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            ClientId = ClientId
        };

        if (Enum.TryParse<SecurityProtocol>(SecurityProtocol, ignoreCase: true, out var securityProtocol))
            config.SecurityProtocol = securityProtocol;

        if (Enum.TryParse<SaslMechanism>(SaslMechanism, ignoreCase: true, out var saslMechanism))
            config.SaslMechanism = saslMechanism;

        if (!string.IsNullOrWhiteSpace(SaslUsername))
            config.SaslUsername = SaslUsername;

        if (!string.IsNullOrWhiteSpace(SaslPassword))
            config.SaslPassword = SaslPassword;

        if (EnableSslCertificateVerification.HasValue)
            config.EnableSslCertificateVerification = EnableSslCertificateVerification.Value;

        return config;
    }
}
