using ECommerce.Core.Aggregates;
using ECommerce.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(produto => produto.ProdutoId);

        builder.Property(produto => produto.ProdutoId)
            .ValueGeneratedNever();

        builder.Property(produto => produto.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(produto => produto.Preco)
            .HasConversion(
                money => money.Valor,
                valor => new Money(valor))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(produto => produto.Ativo)
            .IsRequired();

        builder.Ignore(produto => produto.DomainEvents);
    }
}
