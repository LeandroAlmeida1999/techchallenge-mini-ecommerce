using ECommerce.UseCases.Clientes.Commands;
using ECommerce.UseCases.Clientes.Queries;
using ECommerce.WebApi.Contracts;
using ECommerce.WebApi.Contracts.Requests;

namespace ECommerce.WebApi.Endpoints;

public static class ClientesEndpoints
{
    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/clientes").WithTags("Clientes");

        group.MapPost("/", async (
            CriarClienteRequest request,
            CriarClienteHandler handler,
            CancellationToken cancellationToken) =>
        {
            var cliente = await handler.HandleAsync(
                new CriarClienteCommand(request.Nome, request.Email),
                cancellationToken);

            return Results.Created($"/clientes/{cliente.Id}", cliente.ToResponse());
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ObterClientePorIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var cliente = await handler.HandleAsync(new ObterClientePorIdQuery(id), cancellationToken);
            return Results.Ok(cliente.ToResponse());
        });

        return endpoints;
    }
}
