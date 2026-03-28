namespace ECommerce.WebApi.Contracts.Requests;

public sealed record AdicionarItemPedidoRequest(
    Guid ProdutoId,
    int Quantidade);
