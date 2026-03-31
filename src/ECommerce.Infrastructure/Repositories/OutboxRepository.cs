using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Outbox;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

    public async Task<IReadOnlyCollection<OutboxMessageData>> ReservePendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var reservedMessages = await dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT TOP ({batchSize})
                    [Id],
                    [EventType],
                    [PartitionKey],
                    [Payload],
                    [OccurredOnUtc],
                    [ProcessedOnUtc],
                    [Status],
                    [Error],
                    [Retries]
                FROM [OutboxMessages] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE [ProcessedOnUtc] IS NULL
                  AND [Retries] < {OutboxMessage.MaxRetries}
                  AND [Status] IN ('Pending', 'Failed')
                ORDER BY [OccurredOnUtc]
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in reservedMessages)
        {
            message.MarkAsProcessing();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return reservedMessages
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
            .ToArray();
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
