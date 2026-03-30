using ECommerce.UseCases.Pedidos.Commands;
using ECommerce.UseCases.Pedidos.Queries;
using ECommerce.WebApi.Contracts;
using ECommerce.WebApi.Contracts.Requests;

namespace ECommerce.WebApi.Endpoints;

public static class PedidosEndpoints
{
    public static IEndpointRouteBuilder MapPedidosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/pedidos").WithTags("Pedidos");

        group.MapPost("/", async (
            CriarPedidoRequest request,
            CriarPedidoHandler handler,
            CancellationToken cancellationToken) =>
        {
            var pedido = await handler.HandleAsync(new CriarPedidoCommand(request.ClienteId), cancellationToken);
            return Results.Created($"/pedidos/{pedido.Id}", pedido.ToResponse());
        });

        group.MapPost("/{id:guid}/itens", async (
            Guid id,
            AdicionarItemPedidoRequest request,
            AdicionarItemPedidoHandler handler,
            CancellationToken cancellationToken) =>
        {
            var pedido = await handler.HandleAsync(
                new AdicionarItemPedidoCommand(id, request.ProdutoId, request.Quantidade),
                cancellationToken);

            return Results.Ok(pedido.ToResponse());
        });

        group.MapPost("/{id:guid}/confirmar", async (
            Guid id,
            ConfirmarPedidoHandler handler,
            CancellationToken cancellationToken) =>
        {
            var pedido = await handler.HandleAsync(new ConfirmarPedidoCommand(id), cancellationToken);
            return Results.Ok(pedido.ToResponse());
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ObterPedidoPorIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var pedido = await handler.HandleAsync(new ObterPedidoPorIdQuery(id), cancellationToken);
            return Results.Ok(pedido.ToResponse());
        });

        return endpoints;
    }
}
