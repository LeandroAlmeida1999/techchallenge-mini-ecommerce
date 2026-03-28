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
        .WithTags("Root")
        .WithName("GetApiRoot");

        endpoints.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy"
        }))
        .WithTags("Health")
        .WithName("GetHealth");

        return endpoints;
    }
}
