using Analytics.Application;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common.Middleware;
using Carter;
using Contracts.Currency;
using Contracts.Sale;
using Core.Models;
using Core.StaticFunctions;
using MassTransit;
using Persistence.Extensions;
using RabbitMq;

var builder = WebApplication.CreateBuilder(args);
var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);
builder.Services.AddOpenApi();

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};
builder.Services.AddSingleton(brokerOptions);

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

ConsumerRegistration[] eventHandlers =
[
    new(typeof(CurrencyCreatedEvent), "analytics-queue"),
    new(typeof(CurrencyRateChangedEvent), uniqQueueName),
    new(typeof(SaleCreatedEvent), "analytics-queue"),
    new(typeof(SaleEditedEvent), "analytics-queue"),
    new(typeof(SaleDeletedEvent), "analytics-queue")
];

builder.Services.AddMassageBrokerLayer<DContext>(brokerOptions, eventHandlers, opt =>
    {
        opt.UseBusOutbox();
        opt.UsePostgres();
    })
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddApplicationLayer();

builder.Services.AddCarter();

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

if (Environment.GetEnvironmentVariable("SEED_DB") == "true")
    await app.SeedAsync<DContext>();

app.UseMiddleware<HeaderSecretMiddleware>();


if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();


app.Run();