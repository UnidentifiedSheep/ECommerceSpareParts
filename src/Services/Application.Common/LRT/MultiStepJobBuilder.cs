using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Models.Lrt;

namespace Application.Common.LRT;

internal sealed class MultiStepJobBuilder : IMultiStepJobBuilder
{
	public List<LrtStepDefinition> Steps { get; } = [];

	public LrtStepDefinition AddStep<TInputState>(
		string systemName,
		TInputState inputState,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState
	{
		ArgumentNullException.ThrowIfNull(inputState);
		return AddStep(systemName, _ => inputState, maxAttempts, key);
	}

	public LrtStepDefinition AddStep<TInputState>(
		string systemName,
		Func<LrtStepInputContext, TInputState> inputFactory,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState
	{
		ArgumentNullException.ThrowIfNull(inputFactory);
		return AddStep(systemName, (context, _) => Task.FromResult(inputFactory(context)), maxAttempts, key);
	}

	public LrtStepDefinition AddStep<TInputState>(
		string systemName,
		Func<LrtStepInputContext, CancellationToken, Task<TInputState>> inputFactory,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState
	{
		ArgumentNullException.ThrowIfNull(inputFactory);
		return Add(systemName, key, maxAttempts, typeof(TInputState), async (context, ct) =>
		{
			var input = await inputFactory(context, ct) ??
				throw new InvalidOperationException($"Input factory for '{key ?? systemName}' returned null.");
			return JsonSerializer.Serialize(input);
		});
	}

	public void AddDependency(LrtStepDefinition step, LrtStepDefinition dependsOn)
	{
		if (!Steps.Contains(step) || !Steps.Contains(dependsOn))
			throw new InvalidOperationException("Both steps must belong to this workflow definition.");
		if (!step.Dependencies.Contains(dependsOn))
			step.Dependencies.Add(dependsOn);
	}

	private LrtStepDefinition Add(
		string systemName,
		string? key,
		int? maxAttempts,
		Type inputType, Func<LrtStepInputContext, CancellationToken, Task<string>> inputFactory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(systemName);
		key ??= systemName;
		if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
			throw new ArgumentException("Step key must contain between 1 and 128 characters.", nameof(key));
		if (Steps.Any(x => x.Key == key))
			throw new InvalidOperationException($"Duplicate workflow step key '{key}'.");
		if (maxAttempts is <= 0)
			throw new ArgumentOutOfRangeException(nameof(maxAttempts));
		var step = new LrtStepDefinition(key, systemName, maxAttempts, inputType, inputFactory);
		Steps.Add(step);
		return step;
	}
}
