using ECommerce.Core.Aggregates;
using ECommerce.Core.Interfaces;
using ECommerce.Core.ValueObjects;
using ECommerce.UseCases.DTOs;

namespace ECommerce.UseCases.Clientes.Commands;

public sealed class CriarClienteHandler(IClienteRepository clienteRepository)
{
    public async Task<ClienteDto> HandleAsync(CriarClienteCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var cliente = Cliente.Criar(command.Nome, new Email(command.Email));

        await clienteRepository.AddAsync(cliente, cancellationToken);

        return cliente.ToDto();
    }
}
