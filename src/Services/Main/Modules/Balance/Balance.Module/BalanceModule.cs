using System.Reflection;
using Application.Common.Host;
using Main.Modules.Balance.Application;
using Main.Modules.Balance.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Balance;

public sealed class BalanceModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [BalanceApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddBalanceApplication();
		services.AddBalancePersistence();
	}
}
