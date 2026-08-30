using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Domain.CommonEntities.Job;

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

		if (lrt is not IMultiStepLrt multiStepLrt)
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

		return CreateMultiStepJob(
			multiStepLrt,
			state,
			maxAttempts,
			naturalKey,
			new HashSet<string>(StringComparer.OrdinalIgnoreCase));
	}

	private MultiStepJob CreateMultiStepJob(
		IMultiStepLrt lrt,
		string state,
		int maxAttempts,
		string? naturalKey,
		HashSet<string> compositionPath)
	{
		if (!compositionPath.Add(lrt.SystemName))
			throw new InvalidOperationException(
				$"Multi-step LRT composition contains a cycle at '{lrt.SystemName}'.");

		try
		{
			var job = naturalKey is null
				? MultiStepJob.Create(
					lrt.SystemName,
					state,
					maxAttempts)
				: MultiStepJob.CreateUnique(
					naturalKey,
					lrt.SystemName,
					state,
					maxAttempts);
			var builder = new MultiStepJobBuilder((
				childSystemName, childState,
				childMaxAttempts) => Create(
				job,
				childSystemName,
				childState,
				childMaxAttempts,
				compositionPath));

			lrt.ConfigureSteps(builder, state);

			if (job.Steps.Count == 0)
				throw new InvalidOperationException(
					$"Multi-step LRT '{lrt.SystemName}' must contain at least one step.");

			return job;
		}
		finally
		{
			compositionPath.Remove(lrt.SystemName);
		}
	}

	private Job Create(
		MultiStepJob parent,
		string systemName,
		string inputState,
		int maxAttempts,
		HashSet<string> compositionPath)
	{
		var lrt = registry.GetBySystemName(systemName);
		var state = lrt.ValidateState(inputState);

		Job step;
		if (lrt is not IMultiStepLrt multiStepLrt)
			step = SingleRunJob.Create(
				lrt.SystemName,
				state,
				maxAttempts);
		else
			step = CreateMultiStepJob(
				multiStepLrt,
				state,
				maxAttempts,
				null,
				compositionPath);

		parent.AddStep(step);
		return step;
	}
}
