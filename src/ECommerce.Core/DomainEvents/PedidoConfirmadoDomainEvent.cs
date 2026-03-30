using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.DomainEvents;

public sealed record PedidoConfirmadoDomainEvent(
    Guid PedidoId,
    Guid ClienteId,
    Money ValorTotal,
    DateTime DataConfirmacao) : IDomainEvent
{
    public DateTime OccurredOnUtc => DataConfirmacao;
}
