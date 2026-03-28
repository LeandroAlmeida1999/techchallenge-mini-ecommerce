namespace ECommerce.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
