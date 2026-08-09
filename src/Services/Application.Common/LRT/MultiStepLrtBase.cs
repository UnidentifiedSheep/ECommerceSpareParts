using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class MultiStepLrtBase<TInputState, TState>(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IDomainEventExecutor domainEventExecutor,
    ILogger logger
) : LrtBase<TInputState, TState>(
    jobRepository,
    unitOfWork,
    publisher,
    domainEventExecutor,
    logger), IMultiStepLrt
    where TInputState : class, IInputState
    where TState : class, TInputState
{
    protected internal abstract void ConfigureSteps(
        IMultiStepJobBuilder builder,
        string initialState);

    void IMultiStepLrt.ConfigureSteps(
        IMultiStepJobBuilder builder,
        string initialState)
    {
        ConfigureSteps(builder, initialState);
    }

    protected override Task DoWork()
    {
        return ExecuteWithDomainEventsTransactionAsync(
            TransactionalAttribute.ReadCommited(30, 3),
            ReconcileAsync);
    }

    protected override Task SucceedJobAsync()
    {
        return Job.Status == JobStatus.Waiting
            ? Task.CompletedTask
            : base.SucceedJobAsync();
    }

    private async Task ReconcileAsync()
    {
        var parentCriteria = Criteria<Job>
            .New()
            .Where(x => x.Id == JobId && x is MultiStepJob)
            .Include(x => ((MultiStepJob)x).Dependencies)
            .Track()
            .ForUpdate()
            .Build();

        var parent = await JobRepository.FirstOrDefaultAsync(
                         parentCriteria,
                         CancellationToken)
                     ?? throw new InvalidOperationException(
                         $"Multi-step job with id {JobId} not found.");

        if (parent is not MultiStepJob multiStepJob)
            throw new InvalidOperationException(
                $"Job with id {JobId} is not a multi-step job.");

        var stepCriteria = Criteria<Job>.New()
            .Where(x => x.MultiStepJobId == JobId)
            .Track()
            .ForUpdate()
            .Build();

        var steps = await JobRepository.ListAsync(
            stepCriteria,
            CancellationToken);

        if (steps.Count == 0)
            Interrupt("Multi-step job does not contain any steps.");

        var failedStep = steps.FirstOrDefault(x =>
            x.Status is JobStatus.Failed or JobStatus.Cancelled);

        if (failedStep is not null)
            Interrupt(
                $"Step '{failedStep.SystemName}' finished with status " +
                $"'{failedStep.Status}'.");

        if (steps.All(x => x.Status == JobStatus.Succeeded))
            return;

        var stepsById = steps.ToDictionary(x => x.Id);
        var dependenciesByStepId = multiStepJob.Dependencies
            .ToLookup(x => x.StepId);

        foreach (var step in steps.Where(x => x.Status == JobStatus.Blocked))
        {
            var dependenciesSucceeded = dependenciesByStepId[step.Id].All(dependency =>
                stepsById.TryGetValue(dependency.DependsOnStepId, out var dependsOn) &&
                dependsOn.Status == JobStatus.Succeeded);

            if (dependenciesSucceeded)
                multiStepJob.ActivateStep(step);
        }

        var hasRunnableOrRunningSteps = steps.Any(x =>
            x.Status is JobStatus.Pending or
                JobStatus.Locked or
                JobStatus.Processing or
                JobStatus.Waiting);

        if (!hasRunnableOrRunningSteps)
            Interrupt(
                "Multi-step job cannot make progress because no step is runnable.");

        multiStepJob.Wait(LeaseHolderId);
        await PublishStatusUpdatedEvent(multiStepJob);
        await UnitOfWork.SaveChangesAsync(CancellationToken);
    }
}
