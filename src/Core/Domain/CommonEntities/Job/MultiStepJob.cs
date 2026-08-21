using Domain.CommonEnums;
using Domain.Exceptions;

namespace Domain.CommonEntities.Job;

public sealed class MultiStepJob : Job
{
    private MultiStepJob()
    {
    }

    private MultiStepJob(
        string systemName,
        string initialState,
        int maxAttempts,
        string? naturalKey)
        : base(
            systemName,
            initialState,
            maxAttempts,
            naturalKey)
    {
    }

    private readonly List<Job> _steps = [];
    private readonly List<JobStepDependency> _dependencies = [];

    public IReadOnlyList<Job> Steps => _steps;
    public IReadOnlyList<JobStepDependency> Dependencies => _dependencies;

    public static MultiStepJob Create(
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        return new MultiStepJob(
            systemName,
            initialState,
            maxAttempts,
            null);
    }

    public static MultiStepJob CreateUnique(
        string naturalKey,
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        ArgumentNullException.ThrowIfNull(naturalKey);

        return new MultiStepJob(
            systemName,
            initialState,
            maxAttempts,
            naturalKey);
    }

    public void AddStep(Job step)
    {
        ArgumentNullException.ThrowIfNull(step);
        EnsureTopologyMutable();

        if (_steps.Any(x => x.Id == step.Id))
            return;

        step.AttachTo(this);
        _steps.Add(step);
    }

    public void AddDependency(
        Job step,
        Job dependsOn)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(dependsOn);

        EnsureTopologyMutable();
        EnsureOwnStep(step);
        EnsureOwnStep(dependsOn);

        if (step.Id == dependsOn.Id)
            throw new InvalidOperationException(
                "Job step cannot depend on itself.");

        if (HasDependencyPath(dependsOn.Id, step.Id))
            throw new InvalidOperationException(
                "Dependency would create a cycle.");

        if (_dependencies.Any(x =>
                x.StepId == step.Id &&
                x.DependsOnStepId == dependsOn.Id))
            return;

        _dependencies.Add(JobStepDependency.Create(
            this,
            step,
            dependsOn));
    }

    public void ActivateStep(Job step)
    {
        ArgumentNullException.ThrowIfNull(step);
        EnsureOwnStep(step);
        step.Activate(Id);
    }

    public void CancelUnfinishedSteps(
        IEnumerable<Job> steps,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(steps);

        foreach (var step in steps)
        {
            ArgumentNullException.ThrowIfNull(step);

            if (step.MultiStepJobId != Id)
                throw new InvalidOperationException(
                    "Job step does not belong to this multi-step job.");

            step.CancelBy(this, reason);
        }
    }

    public void Wait(Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);

        if (IsCancellationRequested)
            throw new JobCancellationRequestedException(Id);

        EnsureStatus(JobStatus.Processing);

        SetStatus(JobStatus.Waiting);
        ClearLease();
    }

    public void Resume()
    {
        EnsureStatus(JobStatus.Waiting);
        SetStatus(JobStatus.Pending);
    }

    private void EnsureTopologyMutable()
    {
        if (IsStep)
            throw new InvalidOperationException(
                "Nested multi-step job topology cannot be changed.");

        if (Status != JobStatus.Pending || LockedAt is not null)
            throw new InvalidOperationException(
                "Multi-step job topology cannot be " +
                "changed after execution has started.");
    }

    private void EnsureOwnStep(Job step)
    {
        if (step.MultiStepJobId != Id || _steps.All(x => x.Id != step.Id))
            throw new InvalidOperationException(
                "Job step does not belong to this multi-step job.");
    }

    private bool HasDependencyPath(Guid fromStepId, Guid targetStepId)
    {
        var stepsById = _steps.ToDictionary(x => x.Id);

        var visited = new HashSet<Guid>();
        var stack = new Stack<Guid>();

        stack.Push(fromStepId);

        while (stack.TryPop(out var currentStepId))
        {
            if (!visited.Add(currentStepId))
                continue;

            if (currentStepId == targetStepId)
                return true;

            if (!stepsById.ContainsKey(currentStepId))
                throw new InvalidOperationException(
                    "Workflow contains a dependency on an unknown job step.");

            foreach (var dependency in _dependencies.Where(x =>
                         x.StepId == currentStepId))
                stack.Push(dependency.DependsOnStepId);
        }

        return false;
    }
}
