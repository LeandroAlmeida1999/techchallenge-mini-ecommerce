using ECommerce.Core.Interfaces;
using ECommerce.UseCases.DTOs;

namespace ECommerce.UseCases.Produtos.Queries;

public sealed class ListarProdutosHandler(IProdutoRepository produtoRepository)
{
    public async Task<IReadOnlyCollection<ProdutoDto>> HandleAsync(ListarProdutosQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var produtos = await produtoRepository.ListAsync(cancellationToken);

        return produtos.Select(produto => produto.ToDto()).ToArray();
    }
}
