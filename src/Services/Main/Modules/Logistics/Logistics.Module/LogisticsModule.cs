using Application.Common.Host;
using Main.Modules.Logistics.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Logistics;

public sealed class LogisticsModule : ModuleBase
{
	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		=> services.AddLogisticsPersistence();
}
