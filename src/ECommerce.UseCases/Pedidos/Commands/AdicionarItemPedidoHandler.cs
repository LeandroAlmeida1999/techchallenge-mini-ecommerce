using ECommerce.Core.DomainServices;
using ECommerce.Core.Interfaces;
using ECommerce.Core.ValueObjects;
using ECommerce.UseCases.DTOs;
using ECommerce.UseCases.Exceptions;

namespace ECommerce.UseCases.Pedidos.Commands;

public sealed class AdicionarItemPedidoHandler(
    IPedidoRepository pedidoRepository,
    IProdutoRepository produtoRepository,
    CalculadoraPedidoDomainService calculadoraPedidoDomainService)
{
    public async Task<PedidoDto> HandleAsync(AdicionarItemPedidoCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await pedidoRepository.GetByIdAsync(command.PedidoId, cancellationToken);

        if (pedido is null)
            throw new NotFoundException($"Pedido '{command.PedidoId}' nao encontrado.");

        var produto = await produtoRepository.GetByIdAsync(command.ProdutoId, cancellationToken);

        if (produto is null)
            throw new NotFoundException($"Produto '{command.ProdutoId}' nao encontrado.");

        pedido.AdicionarItem(produto, new Quantidade(command.Quantidade), calculadoraPedidoDomainService);

        await pedidoRepository.UpdateAsync(pedido, cancellationToken);

        return pedido.ToDto();
    }
}
