using Domain.CommonEntities;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Platform.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPlatformPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<PlatformDbContextModule>().AddEntities(typeof(Setting).Assembly);
}
