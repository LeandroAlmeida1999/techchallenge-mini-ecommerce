using ECommerce.Core.Interfaces;
using ECommerce.UseCases.DTOs;
using ECommerce.UseCases.Exceptions;

namespace ECommerce.UseCases.Pedidos.Queries;

public sealed class ObterPedidoPorIdHandler(IPedidoRepository pedidoRepository)
{
    public async Task<PedidoDto> HandleAsync(ObterPedidoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pedido = await pedidoRepository.GetByIdAsync(query.PedidoId, cancellationToken);

        if (pedido is null)
            throw new NotFoundException($"Pedido '{query.PedidoId}' nao encontrado.");

        return pedido.ToDto();
    }
}
