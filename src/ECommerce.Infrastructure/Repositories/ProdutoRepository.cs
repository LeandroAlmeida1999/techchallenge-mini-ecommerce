using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public sealed class ProdutoRepository(ECommerceDbContext dbContext) : IProdutoRepository
{
    public async Task AddAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        await dbContext.Produtos.AddAsync(produto, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Produto?> GetByIdAsync(Guid produtoId, CancellationToken cancellationToken = default)
    {
        return dbContext.Produtos
            .SingleOrDefaultAsync(produto => produto.ProdutoId == produtoId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Produto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Produtos
            .OrderBy(produto => produto.Nome)
            .ToArrayAsync(cancellationToken);
    }
}
