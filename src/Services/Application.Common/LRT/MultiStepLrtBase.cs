using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class MultiStepLrtBase<TInputState, TState>(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger logger) : LrtBase<TInputState, TState>(
		jobRepository,
		unitOfWork,
		publisher,
		transactionService,
		logger),
	IMultiStepLrt where TInputState : class, IInputState where TState : class, TInputState
{

	void IMultiStepLrt.ConfigureSteps(IMultiStepJobBuilder builder, string initialState) =>
		ConfigureSteps(builder, initialState);
	protected internal abstract void ConfigureSteps(IMultiStepJobBuilder builder, string initialState);

	protected override async Task DoWork()
	{
		var failure = await TransactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			(_, _) => ReconcileAsync(),
			CancellationToken);

		if (failure is not null)
			Interrupt(failure);
	}

	protected override Task SucceedJobAsync()
	{
		return Job.Status == JobStatus.Waiting ? Task.CompletedTask : base.SucceedJobAsync();
	}

	private async Task<string?> ReconcileAsync()
	{
		var parentCriteria = Criteria<Job>
			.New()
			.Where(x => x.Id == JobId && x is MultiStepJob)
			.Include(x => ((MultiStepJob)x).Dependencies)
			.Track()
			.ForUpdate()
			.Build();

		var parent = await JobRepository.FirstOrDefaultAsync(parentCriteria, CancellationToken) ??
			throw new InvalidOperationException($"Multi-step job with id {JobId} not found.");

		if (parent is not MultiStepJob multiStepJob)
			throw new InvalidOperationException($"Job with id {JobId} is not a multi-step job.");

		var stepCriteria = Criteria<Job>
			.New()
			.Where(x => x.MultiStepJobId == JobId)
			.Track()
			.ForUpdate()
			.Build();

		var steps = await JobRepository.ListAsync(stepCriteria, CancellationToken);

		if (steps.Count == 0)
			Interrupt("Multi-step job does not contain any steps.");

		var failedStep = steps.FirstOrDefault(x => x.Status is JobStatus.Failed or JobStatus.Cancelled);

		if (failedStep is not null)
		{
			var failure = $"Step '{failedStep.SystemName}' finished with status " + $"'{failedStep.Status}'.";

			await CancelUnfinishedWorkflowAsync(
				multiStepJob,
				steps,
				failure);
			await UnitOfWork.SaveChangesAsync(CancellationToken);
			return failure;
		}

		if (steps.All(x => x.Status == JobStatus.Succeeded))
			return null;

		var stepsById = steps.ToDictionary(x => x.Id);
		var dependenciesByStepId = multiStepJob.Dependencies.ToLookup(x => x.StepId);

		foreach (var step in steps.Where(x => x.Status == JobStatus.Blocked))
		{
			var dependenciesSucceeded = dependenciesByStepId[step.Id]
				.All(dependency => stepsById.TryGetValue(dependency.DependsOnStepId, out var dependsOn) &&
					dependsOn.Status == JobStatus.Succeeded);

			if (dependenciesSucceeded)
				multiStepJob.ActivateStep(step);
		}

		var hasRunnableOrRunningSteps = steps.Any(x =>
			x.Status is JobStatus.Pending or JobStatus.Locked or JobStatus.Processing or JobStatus.Waiting);

		if (!hasRunnableOrRunningSteps)
			Interrupt("Multi-step job cannot make progress because no step is runnable.");

		multiStepJob.Wait(LeaseHolderId);
		await UnitOfWork.SaveChangesAsync(CancellationToken);
		return null;
	}

	private async Task CancelUnfinishedWorkflowAsync(
		MultiStepJob root,
		IReadOnlyCollection<Job> rootSteps,
		string reason)
	{
		var parents = new[]
		{
			root
		};
		var steps = rootSteps;

		while (parents.Length != 0)
		{
			var parentsById = parents.ToDictionary(x => x.Id);

			foreach (var group in steps.GroupBy(x => x.MultiStepJobId))
			{
				if (!group.Key.HasValue || !parentsById.TryGetValue(group.Key.Value, out var parent))
					throw new InvalidOperationException(
						"Multi-step workflow contains a step with an unknown parent.");

				parent.CancelUnfinishedSteps(group, reason);
			}

			parents = steps.OfType<MultiStepJob>().ToArray();

			if (parents.Length == 0)
				break;

			var parentIds = parents.Select(x => x.Id).ToList();
			var criteria = Criteria<Job>
				.New()
				.Where(x => x.MultiStepJobId.HasValue && parentIds.Contains(x.MultiStepJobId.Value))
				.Track()
				.ForUpdate()
				.Build();

			steps = await JobRepository.ListAsync(criteria, CancellationToken);
		}
	}
}
