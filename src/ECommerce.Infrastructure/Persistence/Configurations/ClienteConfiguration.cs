using ECommerce.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.ClienteId);

        builder.Property(cliente => cliente.ClienteId)
            .ValueGeneratedNever();

        builder.Property(cliente => cliente.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(cliente => cliente.Email)
            .HasConversion(email => email.Valor, valor => new(valor))
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cliente => cliente.DataCadastro)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Ignore(cliente => cliente.DomainEvents);
    }
}
