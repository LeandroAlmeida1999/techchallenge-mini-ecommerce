namespace ECommerce.Core.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessageData message, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OutboxMessageData>> ReservePendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessageData message, CancellationToken cancellationToken = default);
}
