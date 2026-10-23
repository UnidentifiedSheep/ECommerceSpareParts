using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Lrt;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using NamedObject.Core.Interfaces;

namespace Application.Common.LRT;

public sealed class JobStepInputInitializer(
	INamedObjectRegistry<ILrtNamedObject> registry
	) : IJobStepInputInitializer
{
	public async Task<Job> InitializeAsync(
		Job step,
		Guid leaseHolderId,
		IApplicationTransactionContext context,
		CancellationToken cancellationToken)
	{
		if (step.MultiStepJobId is not { } parentId || step.StepKey is null)
			throw new InvalidOperationException("Deferred input requires a named workflow step.");

		var repository = context.Repositories.Get<Job, Guid>();

		var parent = await repository.FirstOrDefaultAsync(
				Criteria<Job>.New()
					.Where(x => x.Id == parentId)
					.Include(x => ((MultiStepJob)x).Dependencies)
					.Track()
					.ForUpdate()
					.Build(),
				cancellationToken) as MultiStepJob
			?? throw new InvalidOperationException($"Workflow '{parentId}' was not found.");

		var stepId = step.Id;
		step = await repository.FirstOrDefaultAsync(
				Criteria<Job>.New()
					.Where(x => x.Id == stepId)
					.Track()
					.ForUpdate()
					.Build(),
				cancellationToken)
			?? throw new InvalidOperationException($"Step '{stepId}' was not found.");

		step.EnsureActiveLease(leaseHolderId);
		if (step.InputInitialized) return step;
		step.EnsureStatus(JobStatus.Locked);

		if (parent.Status is JobStatus.Cancelled or JobStatus.CancellationRequested)
		{
			parent.CancelUnfinishedSteps([step], parent.ErrorMessage);
			return step;
		}

		if (parent.IsTerminal || !parent.InputInitialized)
			throw new InvalidOperationException("Workflow is not ready to prepare step input.");

		var dependencyIds = parent.Dependencies
			.Where(x => x.StepId == step.Id)
			.Select(x => x.DependsOnStepId)
			.ToList();

		var dependencies = await repository.ListAsync(
			Criteria<Job>
				.New()
				.Where(x => x.MultiStepJobId == parent.Id && dependencyIds.Contains(x.Id))
				.Build(),
			cancellationToken);

		if (dependencies.Count != dependencyIds.Count || dependencies.Any(x => x.Status != JobStatus.Succeeded))
			throw new InvalidOperationException("All dependencies must succeed before preparing step input.");

		var parentLrt = registry.GetBySystemName(parent.SystemName);
		if (parentLrt is not IMultiStepLrt multiStepLrt)
			throw new InvalidOperationException($"LRT '{parent.SystemName}' is not a workflow.");

		var builder = new MultiStepJobBuilder();
		multiStepLrt.ConfigureSteps(builder);
		var definition = builder.Steps.SingleOrDefault(x => x.Key == step.StepKey) ??
			throw new InvalidOperationException($"Workflow no longer defines step '{step.StepKey}'.");

		var child = registry.GetBySystemName(step.SystemName);
		var dependencyKeys = dependencies.Select(GetStepKey).ToHashSet(StringComparer.Ordinal);

		if (!string.Equals(definition.SystemName, step.SystemName, StringComparison.OrdinalIgnoreCase) ||
			definition.InputType != child.InputType ||
			!dependencyKeys.SetEquals(definition.Dependencies.Select(x => x.Key)))
			throw new InvalidOperationException($"Definition of step '{step.StepKey}' differs from the saved workflow.");

		var outputs = dependencies
			.ToDictionary(GetStepKey, x => (x, registry.GetBySystemName(x.SystemName).OutputType));

		var inputContext = new LrtStepInputContext(parent.Id, parent.State, parentLrt.InputType, outputs, context);
		var input = child.ValidateState(await definition.InputFactory(inputContext, cancellationToken));
		step.InitializeInput(LrtStateSerializer.SerializeInput(input), leaseHolderId);
		return step;
	}

	private static string GetStepKey(Job step) => step.StepKey ?? throw new InvalidOperationException("Dependency has no stable step key.");
}
