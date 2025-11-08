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
Certs.RegisterCerts("/app/certs");
builder.Services.AddOpenApi();

var brokerOptions = new MessageBrokerOptions
{
    Host = builder.Configuration["RabbitMqSettings:Host"]!,
    Username = builder.Configuration["RabbitMqSettings:Username"]!,
    Password = builder.Configuration["RabbitMqSettings:Password"]!
};

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
    .AddPersistenceLayer(builder.Configuration["ConnectionStrings:DefaultConnection"]!)
    .AddApplicationLayer();

builder.Services.AddCarter();

var secret = builder.Configuration["Gateway:Secret"]!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

await app.EnsureDbExists<DContext>();

app.UseMiddleware<HeaderSecretMiddleware>();


if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();


app.Run();