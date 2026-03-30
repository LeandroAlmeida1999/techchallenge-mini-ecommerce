using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.Entities;

public sealed class ItemPedido
{
    private ItemPedido()
    {
    }

    public Guid ProdutoId { get; private set; }
    public Quantidade Quantidade { get; private set; }
    public Money PrecoUnitario { get; private set; }
    public Money Subtotal => PrecoUnitario.Multiplicar(Quantidade);

    public ItemPedido(Guid produtoId, Quantidade quantidade, Money precoUnitario)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("ProdutoId deve ser informado.", nameof(produtoId));

        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    public void AtualizarQuantidade(Quantidade quantidade)
    {
        Quantidade = quantidade;
    }
}
