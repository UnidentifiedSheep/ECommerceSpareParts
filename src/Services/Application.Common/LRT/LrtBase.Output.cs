using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class LrtBase<TInputState, TState, TOutput>(
	IApplicationTransactionFactory transactionFactory, ILogger logger)
	: LrtBase<TInputState, TState>(transactionFactory, logger)
	where TInputState : class, IInputState where TState : class where TOutput : class
{
	public sealed override Type OutputType => typeof(TOutput);

	protected sealed override async Task<string?> SerializeOutputAsync()
	{
		var output = await CreateOutputAsync() ??
			throw new InvalidOperationException($"LRT '{SystemName}' returned null output.");
		return JsonSerializer.Serialize(output);
	}

	protected abstract Task<TOutput> CreateOutputAsync();
}
