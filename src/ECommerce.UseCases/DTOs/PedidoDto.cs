namespace ECommerce.UseCases.DTOs;

public sealed record PedidoDto(
    Guid Id,
    Guid ClienteId,
    string Status,
    DateTime DataCriacao,
    decimal ValorTotal,
    IReadOnlyCollection<ItemPedidoDto> Itens);
