using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Persistence.Listening;

public static class ListeningServiceCollectionExtensions
{
	public static IServiceCollection AddChannelListening(this IServiceCollection services)
	{
		services.TryAddSingleton<NpgsqlChannelListenerHub>();
		services.TryAddSingleton<IChannelListenerFactory>(provider =>
			provider.GetRequiredService<NpgsqlChannelListenerHub>());
		services.TryAddSingleton<IChannelNotifier, NpgsqlChannelNotifier>();

		return services;
	}
}
