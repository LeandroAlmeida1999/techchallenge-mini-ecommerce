using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.UseCases.DTOs;
using ECommerce.UseCases.Exceptions;

namespace ECommerce.UseCases.Pedidos.Commands;

public sealed class CriarPedidoHandler(
    IClienteRepository clienteRepository,
    IPedidoRepository pedidoRepository)
{
    public async Task<PedidoDto> HandleAsync(CriarPedidoCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var cliente = await clienteRepository.GetByIdAsync(command.ClienteId, cancellationToken);

        if (cliente is null)
            throw new NotFoundException($"Cliente '{command.ClienteId}' nao encontrado.");

        var pedido = Pedido.Criar(command.ClienteId);

        await pedidoRepository.AddAsync(pedido, cancellationToken);

        return pedido.ToDto();
    }
}
