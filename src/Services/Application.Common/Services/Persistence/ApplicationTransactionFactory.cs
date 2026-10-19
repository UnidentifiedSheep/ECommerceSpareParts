using Application.Common.Interfaces.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Services.Persistence;

public sealed class ApplicationTransactionFactory(IServiceScopeFactory scopeFactory) : IApplicationTransactionFactory
{
	public IApplicationTransaction Create()
	{
		var context = new OwnedApplicationTransactionContext(scopeFactory.CreateAsyncScope());
		try
		{
			var transaction = context.GetRequiredService<ApplicationTransaction>();
			transaction.SetOwnedContext(context);
			return transaction;
		}
		catch
		{
			context.DisposeAsync().AsTask().GetAwaiter().GetResult();
			throw;
		}
	}
}
