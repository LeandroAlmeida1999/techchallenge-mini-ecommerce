using ECommerce.Core.Aggregates;

namespace ECommerce.Core.Interfaces;

public interface IProdutoRepository
{
    Task AddAsync(Produto produto, CancellationToken cancellationToken = default);
    Task<Produto?> GetByIdAsync(Guid produtoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Produto>> ListAsync(CancellationToken cancellationToken = default);
}
