using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Domain.CommonEntities.Job;

namespace Application.Common.Models.Lrt;

public sealed class LrtStepInputContext
{
	private readonly string _parentState;
	private readonly Type _parentInputType;
	private readonly IReadOnlyDictionary<string, (Job Job, Type? OutputType)> _dependencies;

	internal LrtStepInputContext(
		Guid parentId,
		string parentState,
		Type parentInputType,
		IReadOnlyDictionary<string, (Job Job, Type? OutputType)> dependencies,
		IApplicationTransactionContext transaction)
	{
		ParentJobId = parentId;
		_parentState = parentState;
		_parentInputType = parentInputType;
		_dependencies = dependencies;
		Transaction = transaction;
	}

	public Guid ParentJobId { get; }
	public IApplicationTransactionContext Transaction { get; }

	public TInputState GetParentInput<TInputState>() where TInputState : class, IInputState
	{
		if (_parentInputType != typeof(TInputState))
			throw new InvalidOperationException($"Parent input is not '{typeof(TInputState).Name}'.");
		return LrtStateSerializer.DeserializeInput<TInputState>(_parentState);
	}

	public TOutput GetOutput<TOutput>(LrtStepDefinition step) where TOutput : class
	{
		ArgumentNullException.ThrowIfNull(step);
		if (!_dependencies.TryGetValue(step.Key, out var dependency) ||
			!string.Equals(dependency.Job.SystemName, step.SystemName, StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException($"Step '{step.Key}' is not a declared dependency.");
		if (dependency.OutputType != typeof(TOutput))
			throw new InvalidOperationException($"Step '{step.Key}' does not produce '{typeof(TOutput).Name}'.");
		if (dependency.Job.Output is not { } output)
			throw new InvalidOperationException($"Step '{step.Key}' has no saved output.");

		return JsonSerializer.Deserialize<TOutput>(output) ??
			throw new InvalidOperationException($"Step '{step.Key}' output is empty.");
	}
}
