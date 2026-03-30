using ECommerce.Core.Entities;
using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.DomainServices;

public sealed class CalculadoraPedidoDomainService
{
    public Money CalcularTotal(IEnumerable<ItemPedido> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var total = Money.Zero;

        foreach (var item in itens)
            total += item.Subtotal;

        return total;
    }
}
