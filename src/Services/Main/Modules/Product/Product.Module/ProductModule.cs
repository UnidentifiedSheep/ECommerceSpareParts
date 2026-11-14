using System.Reflection;
using Application.Common.Host;
using Main.Modules.Product.Application;
using Main.Modules.Product.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Product;

public sealed class ProductModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [ProductApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddProductApplication();
		services.AddProductPersistence();
	}
}
