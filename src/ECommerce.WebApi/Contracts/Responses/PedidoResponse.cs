namespace ECommerce.WebApi.Contracts.Responses;

public sealed record PedidoResponse(
    Guid Id,
    Guid ClienteId,
    string Status,
    DateTime DataCriacao,
    decimal ValorTotal,
    IReadOnlyCollection<ItemPedidoResponse> Itens);
