namespace ECommerce.UseCases.DTOs;

public sealed record ProdutoDto(
    Guid Id,
    string Nome,
    decimal Preco,
    bool Ativo);
