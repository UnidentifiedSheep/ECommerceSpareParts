using System.Reflection;
using Application.Common.Host;
using Main.Modules.Analytics.Application;
using Main.Modules.Analytics.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Analytics;

public sealed class AnalyticsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [AnalyticsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddAnalyticsApplication();
		services.AddAnalyticsPersistence();
	}
}
