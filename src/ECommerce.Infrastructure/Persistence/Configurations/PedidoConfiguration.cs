using ECommerce.Core.Aggregates;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(pedido => pedido.PedidoId);

        builder.Property(pedido => pedido.PedidoId)
            .ValueGeneratedNever();

        builder.Property(pedido => pedido.ClienteId)
            .IsRequired();

        builder.Property(pedido => pedido.Status)
            .HasConversion(
                status => status.ToString(),
                valor => Enum.Parse<StatusPedido>(valor))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(pedido => pedido.DataCriacao)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(pedido => pedido.ValorTotal)
            .HasConversion(
                money => money.Valor,
                valor => new Money(valor))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.OwnsMany(pedido => pedido.Itens, itens =>
        {
            itens.ToTable("PedidoItens");

            itens.WithOwner().HasForeignKey("PedidoId");

            itens.HasKey("PedidoId", nameof(ItemPedido.ProdutoId));

            itens.Property(item => item.ProdutoId)
                .ValueGeneratedNever();

            itens.Property(item => item.Quantidade)
                .HasConversion(
                    quantidade => quantidade.Valor,
                    valor => new Quantidade(valor))
                .IsRequired();

            itens.Property(item => item.PrecoUnitario)
                .HasConversion(
                    money => money.Valor,
                    valor => new Money(valor))
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            itens.Ignore(item => item.Subtotal);
        });

        builder.Navigation(pedido => pedido.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(pedido => pedido.DomainEvents);
    }
}
