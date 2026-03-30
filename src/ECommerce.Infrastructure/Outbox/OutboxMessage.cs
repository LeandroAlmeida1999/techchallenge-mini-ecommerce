namespace ECommerce.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? Error { get; private set; }
    public int Retries { get; private set; }

    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string eventType, string payload, DateTime occurredOnUtc)
    {
        Id = id;
        EventType = eventType;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public static OutboxMessage Create(Guid id, string eventType, string payload, DateTime occurredOnUtc)
    {
        return new OutboxMessage(id, eventType, payload, occurredOnUtc);
    }

    public void MarkAsProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Status = "Processed";
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Retries++;
        Status = "Failed";
        Error = error;
    }
}
