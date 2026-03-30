using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class ClienteRepository(ECommerceDbContext dbContext) : IClienteRepository
{
    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await dbContext.Clientes.AddAsync(cliente, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return dbContext.Clientes
            .SingleOrDefaultAsync(cliente => cliente.ClienteId == clienteId, cancellationToken);
    }
}
