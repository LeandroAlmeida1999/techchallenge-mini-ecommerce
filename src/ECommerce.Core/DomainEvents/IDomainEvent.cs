namespace ECommerce.Core.DomainEvents;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
