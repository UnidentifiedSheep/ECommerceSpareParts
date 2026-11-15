using System.Reflection;
using Application.Common.Host;
using Main.Modules.Cart.Application;
using Main.Modules.Cart.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Cart;

public sealed class CartModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [CartApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddCartApplication();
		services.AddCartPersistence();
	}
}
