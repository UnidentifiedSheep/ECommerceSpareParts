using Application.Common.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Cache.OneTimeTokens;

public static class OneTimeTokenServiceCollectionExtensions
{
	public static IServiceCollection AddOneTimeTokens(this IServiceCollection services)
		=> services.AddScoped<IOneTimeTokenStore, OneTimeTokenStore>();
}
