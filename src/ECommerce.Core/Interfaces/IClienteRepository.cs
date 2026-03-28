using ECommerce.Core.Aggregates;

namespace ECommerce.Core.Interfaces;

public interface IClienteRepository
{
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
