namespace ECommerce.WebApi.Contracts.Responses;

public sealed record ItemPedidoResponse(
    Guid ProdutoId,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);
