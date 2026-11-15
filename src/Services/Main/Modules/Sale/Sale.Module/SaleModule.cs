using System.Reflection;
using Application.Common.Host;
using Main.Modules.Sale.Application;
using Main.Modules.Sale.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Sale;

public sealed class SaleModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [SaleApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddSaleApplication();
		services.AddSalePersistence();
	}
}
