using Main.Entities.Balance.Balance;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Balance.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBalancePersistence(this IServiceCollection services)
		=> services.AddDbContextModule<BalanceDbContextModule>().AddEntities(typeof(Transaction).Assembly);
}
