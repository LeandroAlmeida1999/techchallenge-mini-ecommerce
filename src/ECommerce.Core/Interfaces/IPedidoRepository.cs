using ECommerce.Core.Aggregates;

namespace ECommerce.Core.Interfaces;

public interface IPedidoRepository
{
    Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default);
    Task<Pedido?> GetByIdAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default);
}
