using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Purchase.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPurchasePersistence(this IServiceCollection services)
		=> services.AddDbContextModule<PurchaseDbContextModule>().AddEntities(typeof(Entities.Purchase.Purchase.Purchase).Assembly);
}
