namespace ECommerce.Core.Interfaces;

public sealed record OutboxMessageData(
    Guid Id,
    string EventType,
    string Payload,
    DateTime OccurredOnUtc,
    DateTime? ProcessedOnUtc,
    string Status,
    string? Error,
    int Retries);
