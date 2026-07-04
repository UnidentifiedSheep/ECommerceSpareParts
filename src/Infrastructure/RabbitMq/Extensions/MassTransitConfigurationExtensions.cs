using Abstractions.Interfaces;
using Contracts.Job;
using Contracts.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitMq.Extensions;

public static class MassTransitConfigurationExtensions
{
    public static void ConfigureRabbitMq(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext ctx)
    {
        var options = ctx.GetRequiredService<IOptions<MessageBrokerOptions>>().Value;
        cfg.Host(
            options.Url,
            h =>
            {
                h.Username(options.Username);
                h.Password(options.Password);
            });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        cfg.Publish<JobStatusUpdatedEvent>(p => { p.ExchangeType = ExchangeType.Direct; });
        cfg.Publish<SettingUpdatedEvent>(p => { p.ExchangeType = ExchangeType.Direct; });
    }

    public static IRabbitMqReceiveEndpointConfigurator BindForService<TEvent>(
        this IRabbitMqReceiveEndpointConfigurator ep,
        IServiceDefinition serviceDefinition,
        string exchangeType = ExchangeType.Direct) where TEvent : class
    {
        ep.Bind<TEvent>(bind =>
        {
            bind.ExchangeType = exchangeType;
            bind.RoutingKey = serviceDefinition.ServiceName;
        });

        return ep;
    }
}