using Core.Interfaces;
using Core.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMq;

public static class ServiceProvider
{
    public static IServiceCollection AddMassageBrokerLayer<TContext>(this IServiceCollection services,
        MessageBrokerOptions options, ConsumerRegistration[] consumers, Action<IEntityFrameworkOutboxConfigurator>? configurator = null) 
        where TContext : DbContext
    {
        services.AddMassTransit(conf =>
        {
            if (configurator != null)
                conf.AddEntityFrameworkOutbox<TContext>(configurator);
            
            
            foreach (var reg in consumers)
            {
                var consumerType = typeof(MassTransitConsumerAdapter<>).MakeGenericType(reg.EventType);
                conf.AddConsumer(consumerType);
            }

            conf.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(options.Host), h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                var grouped = consumers.GroupBy(c => c.QueueName);

                foreach (var group in grouped)
                    cfg.ReceiveEndpoint(group.Key, e =>
                    {
                        foreach (var reg in group)
                        {
                            var consumerType = typeof(MassTransitConsumerAdapter<>).MakeGenericType(reg.EventType);
                            e.ConfigureConsumer(context, consumerType);
                        }
                    });
            });
        });

        services.AddScoped<IMessageBroker, MessageBroker>();
        return services;
    }
}
