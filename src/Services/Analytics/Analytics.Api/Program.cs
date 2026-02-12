using System.Reflection;
using Abstractions.Interfaces.Currency;
using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common.Middleware;
using Carter;
using MassTransit;
using Persistence.Extensions;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Security.Utils;

var builder = WebApplication.CreateBuilder(args);
var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);
builder.Services.AddOpenApi();

builder.Services.AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddApplicationLayer();

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};
builder.Services.AddSingleton(brokerOptions);

builder.Services.AddHttpContextAccessor();

var uniqQueueName = $"queue-of-analytics-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(CurrencyCreatedConsumer)));
    
    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(brokerOptions);
        
        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;
        });
        
        cfg.ReceiveEndpoint("analytics-queue", ep =>
        {
            ep.Durable = true;
            
            ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            ep.ConfigureConsumer<SaleCreatedConsumer>(context);
            ep.ConfigureConsumer<SaleDeletedConsumer>(context);
            ep.ConfigureConsumer<SaleEditedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
        });
    });
});

builder.Services.AddCarter();

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

if (Environment.GetEnvironmentVariable("SEED_DB") == "true")
    await app.SeedAsync<DContext>();

app.UseMiddleware<HeaderSecretMiddleware>();

await SetupCurrency(app.Services);

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true")
    app.UseHttpsRedirection();


app.Run();

async Task SetupCurrency(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var currencyConverterSetup = scope.ServiceProvider.GetRequiredService<ICurrencyConverterSetup>();
    await currencyConverterSetup.InitializeAsync();
}