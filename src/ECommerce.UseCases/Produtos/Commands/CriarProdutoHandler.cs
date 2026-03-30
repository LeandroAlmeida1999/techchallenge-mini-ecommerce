using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.Core.ValueObjects;
using ECommerce.UseCases.DTOs;

namespace ECommerce.UseCases.Produtos.Commands;

public sealed class CriarProdutoHandler(IProdutoRepository produtoRepository)
{
    public async Task<ProdutoDto> HandleAsync(CriarProdutoCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var produto = Produto.Criar(command.Nome, new Money(command.Preco), command.Ativo);

        await produtoRepository.AddAsync(produto, cancellationToken);

        return produto.ToDto();
    }
}
