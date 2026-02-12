using MassTransit;
using RabbitMq.Models;

namespace RabbitMq.Extensions;

public static class MassTransitConfigurationExtensions
{
    public static void ConfigureRabbitMq(this IRabbitMqBusFactoryConfigurator cfg, MessageBrokerOptions options)
    {
        cfg.Host(options.Host, h =>
        {
            h.Username(options.Username);
            h.Password(options.Password);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    }
}