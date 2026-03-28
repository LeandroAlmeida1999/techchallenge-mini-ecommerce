namespace ECommerce.WebApi.Contracts.Requests;

public sealed record CriarProdutoRequest(
    string Nome,
    decimal Preco,
    bool Ativo);
