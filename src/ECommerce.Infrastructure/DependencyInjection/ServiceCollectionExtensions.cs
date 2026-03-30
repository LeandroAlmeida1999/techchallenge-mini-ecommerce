using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Kafka;
using ECommerce.Infrastructure.Outbox;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? "Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<IIntegrationEventPublisher, KafkaIntegrationEventPublisher>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }
}
