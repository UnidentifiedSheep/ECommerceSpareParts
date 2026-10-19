using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Services.Persistence;

public sealed class OwnedApplicationTransactionContext(IServiceScope scope)
	: ApplicationTransactionContext(scope.ServiceProvider)
{
	private IServiceScope? _scope = scope;

	public override T GetRequiredService<T>()
	{
		ObjectDisposedException.ThrowIf(_scope is null, this);
		return base.GetRequiredService<T>();
	}

	public override void Dispose()
	{
		// Closing the scope also disposes its transaction, which calls back into this context.
		Interlocked.Exchange(ref _scope, null)?.Dispose();
		base.Dispose();
	}

	public override async ValueTask DisposeAsync()
	{
		var ownedScope = Interlocked.Exchange(ref _scope, null);
		if (ownedScope is IAsyncDisposable asyncScope)
			await asyncScope.DisposeAsync();
		else
			ownedScope?.Dispose();
		await base.DisposeAsync();
	}
}
