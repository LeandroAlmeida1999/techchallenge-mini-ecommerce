using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static async Task ApplyInfrastructureMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
