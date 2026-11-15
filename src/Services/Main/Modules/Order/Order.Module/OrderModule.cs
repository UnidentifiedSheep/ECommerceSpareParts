using System.Reflection;
using Application.Common.Host;
using Main.Modules.Order.Application;
using Main.Modules.Order.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Order;

public sealed class OrderModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [OrderApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddOrderApplication();
		services.AddOrderPersistence();
	}
}
