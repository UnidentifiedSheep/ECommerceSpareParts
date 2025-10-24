using Analytics.Application;
using Analytics.Application.EventHandlers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common.Middleware;
using Carter;
using Contracts.Currency;
using Contracts.Sale;
using Core.Interfaces.MessageBroker;
using Core.Models;
using MassTransit;
using RabbitMq;

var builder = WebApplication.CreateBuilder(args);

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
    new(typeof(SaleCreatedEvent), "analytics-queue")
];

builder.Services.AddScoped<IEventHandler<CurrencyCreatedEvent>, CurrencyCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<SaleCreatedEvent>, SaleCreatedEventHandler>();

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

app.UseMiddleware<HeaderSecretMiddleware>();


if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();


app.Run();