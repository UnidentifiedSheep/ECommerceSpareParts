using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Models;

namespace RabbitMq.Extensions;

public static class MassTransitConfigurationExtensions
{
    public static void ConfigureRabbitMq(
        this IRabbitMqBusFactoryConfigurator cfg, 
        MessageBrokerOptions brokerOptions)
    {
        cfg.Host(brokerOptions.Host, h =>
        {
            h.Username(brokerOptions.Username);
            h.Password(brokerOptions.Password);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    }
}