using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Outbox;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class PedidoRepository(ECommerceDbContext dbContext) : IPedidoRepository
{
    public async Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        await dbContext.Pedidos.AddAsync(pedido, cancellationToken);
        await PersistOutboxMessagesAsync(pedido, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Pedido?> GetByIdAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        return dbContext.Pedidos
            .Include(pedido => pedido.Itens)
            .SingleOrDefaultAsync(pedido => pedido.PedidoId == pedidoId, cancellationToken);
    }

    public async Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        dbContext.Pedidos.Update(pedido);
        await PersistOutboxMessagesAsync(pedido, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistOutboxMessagesAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        var outboxMessages = pedido.DomainEvents
            .Select(OutboxMessageMapper.FromDomainEvent)
            .ToArray();

        if (outboxMessages.Length > 0)
        {
            await dbContext.OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
            pedido.ClearDomainEvents();
        }
    }
}
