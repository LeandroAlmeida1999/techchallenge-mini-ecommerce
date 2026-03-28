namespace ECommerce.UseCases.Produtos.Commands;

public sealed record CriarProdutoCommand(
    string Nome,
    decimal Preco,
    bool Ativo);
