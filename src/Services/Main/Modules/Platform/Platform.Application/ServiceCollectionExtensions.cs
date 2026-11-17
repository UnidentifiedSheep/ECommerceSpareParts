using Application.Common.Host;
using Application.Common.Interfaces.Links;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Main.Modules.Platform.Application.Services;
using Main.Modules.Platform.Application.Services.Settings;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Modules.Platform.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
	{
		services.AddModuleApplication(PlatformApplicationAssembly.Value);
		services.RegisterNamedObject<SettingDefinitionNamedObjectBase>(PlatformApplicationAssembly.Value);

		services
			.AddFusionCache()
			.WithRegisteredDistributedCache()
			.WithRegisteredBackplane()
			.WithSystemTextJsonSerializer();

		services.AddSingleton<ISettingsContainer, SettingsContainer>();
		services.AddScoped<ISettingsService, SettingsService>();
		services.AddScoped<IAppLinkBuilder, AppLinkBuilder>();

		return services;
	}
}
