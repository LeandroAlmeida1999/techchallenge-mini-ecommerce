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

    public async Task<IReadOnlyCollection<OutboxMessageData>> ReservePendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Database
            .SqlQuery<OutboxMessageData>($"""
                ;WITH reserved AS
                (
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
                )
                UPDATE reserved
                SET
                    [Status] = 'Processing',
                    [Error] = NULL
                OUTPUT
                    INSERTED.[Id] AS [Id],
                    INSERTED.[EventType] AS [EventType],
                    INSERTED.[PartitionKey] AS [PartitionKey],
                    INSERTED.[Payload] AS [Payload],
                    INSERTED.[OccurredOnUtc] AS [OccurredOnUtc],
                    INSERTED.[ProcessedOnUtc] AS [ProcessedOnUtc],
                    INSERTED.[Status] AS [Status],
                    INSERTED.[Error] AS [Error],
                    INSERTED.[Retries] AS [Retries];
                """)
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
