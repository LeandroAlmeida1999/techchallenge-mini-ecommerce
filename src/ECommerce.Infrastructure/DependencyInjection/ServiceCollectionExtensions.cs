using ECommerce.Infrastructure.Persistence;
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

        return services;
    }
}
