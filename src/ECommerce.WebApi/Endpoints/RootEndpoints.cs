namespace ECommerce.WebApi.Endpoints;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => Results.Ok(new
        {
            name = "ECommerce API",
            status = "bootstrapped",
            documentation = "/swagger"
        }))
        .WithName("GetApiRoot")
        .ExcludeFromDescription();

        endpoints.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy"
        }))
        .WithName("GetHealth")
        .ExcludeFromDescription();

        return endpoints;
    }
}
