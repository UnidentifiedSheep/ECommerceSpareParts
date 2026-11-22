using System.Reflection;
using Application.Common.Host;
using Main.Modules.Logistics.Application;
using Main.Modules.Logistics.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Logistics;

public sealed class LogisticsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [LogisticsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddLogisticsApplication();
		services.AddLogisticsPersistence();
	}
}
