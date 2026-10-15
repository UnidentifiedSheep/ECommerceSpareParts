using Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Interfaces;
using Persistence.Services;
using Persistence.Services.UnitOfWork;
using Persistence.TransactionBuilder;

namespace Persistence.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection collection) where T : DbContext
	{
		collection.TryAddSingleton<PostgresTransactionExceptionClassifier>();
		collection.TryAddSingleton<ITransactionExceptionClassifier>(provider =>
			provider.GetRequiredService<PostgresTransactionExceptionClassifier>());
		collection.AddScoped<IUnitOfWork, EfUnitOfWork<T>>();
		collection.AddScoped<IContextMetadata, ContextMetadata<T>>();
		collection.AddScoped<IQueryableExtensions, QueryableExtensions>();
		Application.Common.Extensions.ServiceCollectionExtensions.RegisterApplicationTransactions(collection);

		return collection;
	}
}
