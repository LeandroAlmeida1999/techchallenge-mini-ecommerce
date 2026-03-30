namespace ECommerce.Infrastructure.Kafka;

public sealed record PedidoConfirmadoIntegrationEvent(
    Guid PedidoId,
    Guid ClienteId,
    decimal ValorTotal,
    DateTime DataConfirmacao);
