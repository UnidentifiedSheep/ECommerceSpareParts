using System.Reflection;
using Abstractions;
using Application.Common;
using Application.Common.Host;
using Main.Modules.Platform.Application;
using Main.Modules.Platform.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Platform;

public sealed class PlatformModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [PlatformApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddApplicationBase(ServicesDefinitions.Main, configuration);
		services.AddPlatformApplication();
		services.AddPlatformPersistence();
	}
}
