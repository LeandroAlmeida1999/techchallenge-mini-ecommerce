namespace ECommerce.UseCases.Pedidos.Commands;

public sealed record AdicionarItemPedidoCommand(
    Guid PedidoId,
    Guid ProdutoId,
    int Quantidade);
