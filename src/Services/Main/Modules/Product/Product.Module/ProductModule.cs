using Application.Common.Host;
using Main.Modules.Product.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Product;

public sealed class ProductModule : ModuleBase
{
	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		=> services.AddProductPersistence();
}
