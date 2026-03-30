using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.WebApi.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
