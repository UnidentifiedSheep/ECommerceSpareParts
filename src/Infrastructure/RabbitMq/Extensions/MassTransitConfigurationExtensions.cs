using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RabbitMq.Extensions;

public static class MassTransitConfigurationExtensions
{
    public static void ConfigureRabbitMq(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext ctx)
    {
        var options = ctx.GetRequiredService<IOptions<MessageBrokerOptions>>().Value;
        cfg.Host(options.Url, h =>
        {
            h.Username(options.Username);
            h.Password(options.Password);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    }
}