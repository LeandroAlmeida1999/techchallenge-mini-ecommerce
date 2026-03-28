using ECommerce.UseCases.Produtos.Commands;
using ECommerce.UseCases.Produtos.Queries;
using ECommerce.WebApi.Contracts;
using ECommerce.WebApi.Contracts.Requests;

namespace ECommerce.WebApi.Endpoints;

public static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/produtos").WithTags("Produtos");

        group.MapPost("/", async (
            CriarProdutoRequest request,
            CriarProdutoHandler handler,
            CancellationToken cancellationToken) =>
        {
            var produto = await handler.HandleAsync(
                new CriarProdutoCommand(request.Nome, request.Preco, request.Ativo),
                cancellationToken);

            return Results.Created($"/produtos/{produto.Id}", produto.ToResponse());
        });

        group.MapGet("/", async (
            ListarProdutosHandler handler,
            CancellationToken cancellationToken) =>
        {
            var produtos = await handler.HandleAsync(new ListarProdutosQuery(), cancellationToken);
            return Results.Ok(produtos.Select(produto => produto.ToResponse()));
        });

        return endpoints;
    }
}
