using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Models.Lrt;
using Domain.CommonEntities.Job;
using NamedObject.Core.Interfaces;

namespace Application.Common.LRT;

public sealed class JobCreationDispatcher(INamedObjectRegistry<ILrtNamedObject> registry)
	: IJobCreationDispatcher
{
	public Job Create(
		string systemName,
		string inputState,
		int maxAttempts,
		string? naturalKey = null)
	{
		var lrt = registry.GetBySystemName(systemName);
		var state = lrt.ValidateState(inputState);
		if (lrt is IMultiStepLrt multiStep)
			return CreateMultiStepJob(
				multiStep,
				state,
				maxAttempts,
				naturalKey,
				new HashSet<string>(StringComparer.OrdinalIgnoreCase));

		return naturalKey is null
			? SingleRunJob.Create(
				lrt.SystemName,
				state,
				maxAttempts)
			: SingleRunJob.CreateUnique(
				naturalKey,
				lrt.SystemName,
				state,
				maxAttempts);
	}

	private MultiStepJob CreateMultiStepJob(
		IMultiStepLrt lrt,
		string? state,
		int maxAttempts,
		string? naturalKey,
		HashSet<string> compositionPath)
	{
		if (!compositionPath.Add(lrt.SystemName))
			throw new InvalidOperationException($"Multi-step LRT composition contains a cycle at '{lrt.SystemName}'.");
		try
		{
			var job = state is null
				? MultiStepJob.CreateDeferred(lrt.SystemName, maxAttempts)
				: naturalKey is null
					? MultiStepJob.Create(lrt.SystemName, state)
					: MultiStepJob.CreateUnique(naturalKey, lrt.SystemName, state);

			var builder = new MultiStepJobBuilder();
			lrt.ConfigureSteps(builder);
			if (builder.Steps.Count == 0)
				throw new InvalidOperationException($"Multi-step LRT '{lrt.SystemName}' must contain at least one step.");

			var steps = new Dictionary<LrtStepDefinition, Job>();
			foreach (var definition in builder.Steps)
			{
				var child = registry.GetBySystemName(definition.SystemName);
				if (definition.InputType != child.InputType)
					throw new InvalidOperationException($"Input factory for '{definition.Key}' must return '{child.InputType.Name}'.");
				var attempts = definition.MaxAttempts ?? maxAttempts;
				Job step = child is IMultiStepLrt multiStep
					? CreateMultiStepJob(multiStep, null, attempts, null, compositionPath)
					: SingleRunJob.CreateDeferred(child.SystemName, attempts);
				job.AddStep(step, definition.Key);
				steps.Add(definition, step);
			}
			foreach (var definition in builder.Steps)
				foreach (var dependency in definition.Dependencies)
					job.AddDependency(steps[definition], steps[dependency]);
			return job;
		}
		finally
		{
			compositionPath.Remove(lrt.SystemName);
		}
	}
}
