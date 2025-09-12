using System.Reflection;
using Core.Interfaces;
using Core.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Consumers;

namespace RabbitMq;

public static class ServiceProvider
{
    public static IServiceCollection AddMassageBrokerLayer(this IServiceCollection collection,
        MessageBrokerOptions options)
    {
        var brokerOptions = options;
        collection.AddTransient<IMessageBroker, MessageBroker>();

        collection.AddMassTransit(conf =>
        {
            conf.SetKebabCaseEndpointNameFormatter();
            conf.AddConsumers(Assembly.GetExecutingAssembly());
            conf.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(brokerOptions.Host), x =>
                {
                    x.Username(brokerOptions.Username);
                    x.Password(brokerOptions.Password);
                });
                var queueName = $"currency-rates-updated-{Environment.MachineName}";
                configurator.ReceiveEndpoint(queueName, e =>
                {
                    e.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
                    e.ConfigureConsumer<MarkupGroupChangedConsumer>(context);
                    e.ConfigureConsumer<MarkupRangesChangedConsumer>(context);
                });
            });
        });

        return collection;
    }
}