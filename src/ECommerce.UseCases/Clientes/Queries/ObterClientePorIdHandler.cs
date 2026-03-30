using ECommerce.Core.Interfaces;
using ECommerce.UseCases.DTOs;
using ECommerce.UseCases.Exceptions;

namespace ECommerce.UseCases.Clientes.Queries;

public sealed class ObterClientePorIdHandler(IClienteRepository clienteRepository)
{
    public async Task<ClienteDto> HandleAsync(ObterClientePorIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var cliente = await clienteRepository.GetByIdAsync(query.ClienteId, cancellationToken);

        if (cliente is null)
        {
            throw new NotFoundException($"Cliente '{query.ClienteId}' nao encontrado.");
        }

        return cliente.ToDto();
    }
}
