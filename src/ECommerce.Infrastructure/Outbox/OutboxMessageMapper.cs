using ECommerce.Core.DomainEvents;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Kafka;
using System.Text.Json;

namespace ECommerce.Infrastructure.Outbox;

public static class OutboxMessageMapper
{
    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            PedidoConfirmadoDomainEvent pedidoConfirmado => CreatePedidoConfirmadoOutboxMessage(pedidoConfirmado),
            _ => throw new InvalidOperationException($"Nao existe mapeamento de outbox para o evento '{domainEvent.GetType().Name}'.")
        };
    }

    public static OutboxMessageData ToData(this OutboxMessage message)
    {
        return new OutboxMessageData(
            message.Id,
            message.EventType,
            message.Payload,
            message.OccurredOnUtc,
            message.ProcessedOnUtc,
            message.Status,
            message.Error,
            message.Retries);
    }

    private static OutboxMessage CreatePedidoConfirmadoOutboxMessage(PedidoConfirmadoDomainEvent domainEvent)
    {
        var integrationEvent = new PedidoConfirmadoIntegrationEvent(
            domainEvent.PedidoId,
            domainEvent.ClienteId,
            domainEvent.ValorTotal.Valor,
            domainEvent.DataConfirmacao);

        var payload = JsonSerializer.Serialize(integrationEvent);

        return OutboxMessage.Create(
            Guid.NewGuid(),
            nameof(PedidoConfirmadoIntegrationEvent),
            payload,
            domainEvent.OccurredOnUtc);
    }
}
