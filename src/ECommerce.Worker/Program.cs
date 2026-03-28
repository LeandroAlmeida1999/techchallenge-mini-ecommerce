using ECommerce.Infrastructure.DependencyInjection;
using ECommerce.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();
host.Run();
