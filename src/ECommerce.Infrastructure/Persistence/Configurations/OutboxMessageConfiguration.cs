using ECommerce.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.PartitionKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(message => message.ProcessedOnUtc)
            .HasColumnType("datetime2");

        builder.Property(message => message.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasMaxLength(2000);

        builder.Property(message => message.Retries)
            .IsRequired();

        builder.HasIndex(message => new
            {
                message.ProcessedOnUtc,
                message.Retries,
                message.OccurredOnUtc
            })
            .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}
