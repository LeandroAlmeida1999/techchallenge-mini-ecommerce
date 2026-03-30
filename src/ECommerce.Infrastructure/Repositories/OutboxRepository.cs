using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Outbox;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class OutboxRepository(ECommerceDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessageData message, CancellationToken cancellationToken = default)
    {
        await dbContext.OutboxMessages.AddAsync(
            OutboxMessage.Create(message.Id, message.EventType, message.PartitionKey, message.Payload, message.OccurredOnUtc),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OutboxMessageData>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.Retries < OutboxMessage.MaxRetries)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(batchSize)
            .Select(message => new OutboxMessageData(
                message.Id,
                message.EventType,
                message.PartitionKey,
                message.Payload,
                message.OccurredOnUtc,
                message.ProcessedOnUtc,
                message.Status,
                message.Error,
                message.Retries))
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpdateAsync(OutboxMessageData message, CancellationToken cancellationToken = default)
    {
        var current = await dbContext.OutboxMessages
            .SingleAsync(outbox => outbox.Id == message.Id, cancellationToken);

        if (message.ProcessedOnUtc.HasValue)
        {
            current.MarkAsProcessed(message.ProcessedOnUtc.Value);
        }
        else if (!string.IsNullOrWhiteSpace(message.Error))
        {
            current.MarkAsFailed(message.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
