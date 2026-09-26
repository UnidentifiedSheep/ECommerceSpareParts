using Gateway.MessageHandlers;

namespace Gateway.Extensions;

public static class FusionHttpClientExtensions
{
	private const string ClientName = "fusion";
	private const string InternalTokenHeader = "X-Internal-Token";

	public static IServiceCollection AddFusionHttpClient(
		this IServiceCollection services,
		string internalToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(internalToken);

		services.AddHttpContextAccessor();

		services.AddTransient<AuthorizationPropagationHandler>();

		services.AddHeaderPropagation(options =>
		{
			options.Headers.Add("Authorization");
		});

		services
			.AddHttpClient(
				ClientName,
				client =>
				{
					client.DefaultRequestHeaders.Add(InternalTokenHeader, internalToken);
				})
			.AddHeaderPropagation()
			.AddHttpMessageHandler<AuthorizationPropagationHandler>();

		return services;
	}
}
