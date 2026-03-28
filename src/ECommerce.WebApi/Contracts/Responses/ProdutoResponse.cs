namespace ECommerce.WebApi.Contracts.Responses;

public sealed record ProdutoResponse(
    Guid Id,
    string Nome,
    decimal Preco,
    bool Ativo);
