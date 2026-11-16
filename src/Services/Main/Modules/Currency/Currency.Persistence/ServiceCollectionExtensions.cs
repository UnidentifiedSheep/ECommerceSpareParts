using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Currency.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCurrencyPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<CurrencyDbContextModule>().AddEntities(typeof(Main.Entities.Currency.Currency).Assembly);
}
