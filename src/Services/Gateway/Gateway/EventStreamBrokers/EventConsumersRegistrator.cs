using System.Reflection;
using MassTransit;

namespace Gateway.EventStreamBrokers;

public static class EventConsumersRegistrator
{
	public static void RegisterConsumers(IBusRegistrationConfigurator configurator)
	{
		foreach (var (eventType, _) in EventRegistry.Instance.ByTypeDescriptors)
			configurator.AddConsumer(GetConsumerType(eventType));
	}

	private static readonly MethodInfo BindMethod =
		typeof(EventConsumersRegistrator)
			.GetMethod(
				nameof(BindConsumer),
				BindingFlags.NonPublic | BindingFlags.Static)!;

	public static void BindConsumers(
		IBusRegistrationContext context,
		IRabbitMqReceiveEndpointConfigurator ep)
	{
		foreach (var (eventType, _) in EventRegistry.Instance.ByTypeDescriptors)
		{
			ep.ConfigureConsumer(context, GetConsumerType(eventType));

			BindMethod
				.MakeGenericMethod(eventType)
				.Invoke(null, [ep]);
		}
	}

	private static void BindConsumer<TEvent>(IRabbitMqReceiveEndpointConfigurator ep) where TEvent : class
		=> ep.Bind<TEvent>();

	private static Type GetConsumerType(Type eventType)
		=> typeof(FusionEventConsumer<>).MakeGenericType(eventType);
}
