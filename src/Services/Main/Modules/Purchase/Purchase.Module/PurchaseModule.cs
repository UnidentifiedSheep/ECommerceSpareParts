using System.Reflection;
using Application.Common.Host;
using Main.Modules.Purchase.Application;
using Main.Modules.Purchase.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Purchase;

public sealed class PurchaseModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [PurchaseApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddPurchaseApplication();
		services.AddPurchasePersistence();
	}
}
