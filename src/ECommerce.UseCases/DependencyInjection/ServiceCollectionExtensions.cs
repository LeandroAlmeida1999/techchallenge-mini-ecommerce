using ECommerce.Core.DomainServices;
using ECommerce.UseCases.Clientes.Commands;
using ECommerce.UseCases.Clientes.Queries;
using ECommerce.UseCases.Produtos.Commands;
using ECommerce.UseCases.Produtos.Queries;
using ECommerce.UseCases.Pedidos.Commands;
using ECommerce.UseCases.Pedidos.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.UseCases.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<CalculadoraPedidoDomainService>();
        services.AddScoped<CriarClienteHandler>();
        services.AddScoped<ObterClientePorIdHandler>();
        services.AddScoped<CriarProdutoHandler>();
        services.AddScoped<ListarProdutosHandler>();
        services.AddScoped<CriarPedidoHandler>();
        services.AddScoped<AdicionarItemPedidoHandler>();
        services.AddScoped<ConfirmarPedidoHandler>();
        services.AddScoped<ObterPedidoPorIdHandler>();

        return services;
    }
}
