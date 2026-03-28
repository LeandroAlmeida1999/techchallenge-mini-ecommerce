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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapRootEndpoints();

app.Run();
