using Analytics.Application.EventHandlers;
using Contracts.Currency;
using Core.Interfaces.MessageBroker;
using Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

ConsumerRegistration[] eventHandlers =
[
    new(typeof(CurrencyCreatedEvent), "currency-queue"),
    new(typeof(CurrencyRateChangedEvent), uniqQueueName)
];

/*builder.Services.AddScoped<IEventHandler<CurrencyCreatedEvent>, CurrencyCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();*/

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();


app.Run();