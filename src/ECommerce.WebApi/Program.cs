using ECommerce.Infrastructure.DependencyInjection;
using ECommerce.UseCases.DependencyInjection;
using ECommerce.WebApi.DependencyInjection;
using ECommerce.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddUseCases()
    .AddWebApi()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyInfrastructureMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapRootEndpoints();
app.MapClientesEndpoints();
app.MapProdutosEndpoints();
app.MapPedidosEndpoints();

app.Run();
