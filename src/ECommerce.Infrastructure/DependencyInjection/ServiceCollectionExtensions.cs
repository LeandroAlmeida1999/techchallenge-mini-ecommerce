using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Kafka;
using ECommerce.Infrastructure.Outbox;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string KafkaSectionNameConfigPath = "KafkaSettings:SectionName";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? "Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";
        var kafkaSectionName = configuration[KafkaSectionNameConfigPath]
            ?? throw new InvalidOperationException("KafkaSettings:SectionName deve ser configurado.");

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(kafkaSectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka:BootstrapServers deve ser configurado.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Topic), "Kafka:Topic deve ser configurado.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Kafka:ClientId deve ser configurado.")
            .ValidateOnStart();

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<IIntegrationEventPublisher, KafkaIntegrationEventPublisher>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }
}
