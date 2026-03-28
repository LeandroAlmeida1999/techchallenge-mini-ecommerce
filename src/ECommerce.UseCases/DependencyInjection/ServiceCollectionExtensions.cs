using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.UseCases.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services;
    }
}
