using ECommerce.Core.DomainServices;
using ECommerce.Core.Interfaces;
using ECommerce.UseCases.DTOs;
using ECommerce.UseCases.Exceptions;

namespace ECommerce.UseCases.Pedidos.Commands;

public sealed class ConfirmarPedidoHandler(
    IPedidoRepository pedidoRepository,
    CalculadoraPedidoDomainService calculadoraPedidoDomainService,
    ValidadorConfirmacaoPedidoDomainService validadorConfirmacaoPedidoDomainService)
{
    public async Task<PedidoDto> HandleAsync(ConfirmarPedidoCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await pedidoRepository.GetByIdAsync(command.PedidoId, cancellationToken);

        if (pedido is null)
            throw new NotFoundException($"Pedido '{command.PedidoId}' nao encontrado.");

        pedido.Confirmar(calculadoraPedidoDomainService, validadorConfirmacaoPedidoDomainService);

        await pedidoRepository.UpdateAsync(pedido, cancellationToken);

        return pedido.ToDto();
    }
}
