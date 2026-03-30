namespace ECommerce.UseCases.DTOs;

public sealed record ItemPedidoDto(
    Guid ProdutoId,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);
