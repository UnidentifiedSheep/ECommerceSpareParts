using System.Reflection;
using MassTransit;
using RabbitMq.Extensions;

namespace Gateway.EventStreamBrokers;

public static class EventConsumersRegistrator
{

	private static readonly MethodInfo BindMethod =
		typeof(EventConsumersRegistrator)
			.GetMethod(
				nameof(BindConsumer),
				BindingFlags.NonPublic | BindingFlags.Static)!;

	public static void RegisterConsumers(IBusRegistrationConfigurator configurator)
	{
		foreach (var (eventType, _) in EventRegistry.Instance.ByTypeDescriptors)
			configurator.AddConsumer(GetConsumerType(eventType));
	}

	public static void BindConsumers(
		IBusRegistrationContext context,
		IRabbitMqReceiveEndpointConfigurator ep)
	{
		foreach (var (eventType, descriptor) in EventRegistry.Instance.ByTypeDescriptors)
		{
			ep.ConfigureConsumer(context, GetConsumerType(eventType));

			BindMethod
				.MakeGenericMethod(eventType)
				.Invoke(null, [ep, descriptor]);
		}
	}

	private static void BindConsumer<TEvent>(
		IRabbitMqReceiveEndpointConfigurator ep,
		FusionEventDescriptor descriptor) where TEvent : class
	{
		if (descriptor.RoutedFrom.Count == 0)
		{
			ep.Bind<TEvent>();
			return;
		}

		foreach (var service in descriptor.RoutedFrom)
			ep.BindForService<TEvent>(service);
	}

	private static Type GetConsumerType(Type eventType)
		=> typeof(FusionEventConsumer<>).MakeGenericType(eventType);
}
