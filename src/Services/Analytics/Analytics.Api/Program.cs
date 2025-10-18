using Analytics.Application;
using Analytics.Application.EventHandlers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Contracts.Currency;
using Contracts.Sale;
using Core.Interfaces.MessageBroker;
using Core.Models;
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

builder.Services.AddMassageBrokerLayer<DContext>(brokerOptions, eventHandlers)
    .AddPersistenceLayer(builder.Configuration["ConnectionStrings:DefaultConnection"]!)
    .AddApplicationLayer();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();


app.Run();