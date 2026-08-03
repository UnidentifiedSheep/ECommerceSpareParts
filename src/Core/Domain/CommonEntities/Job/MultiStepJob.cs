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
        int maxAttempts)
        : base(
            systemName,
            initialState,
            maxAttempts)
    {
    }

    private readonly List<JobStep> _steps = [];

    public IReadOnlyList<JobStep> Steps => _steps;

    public static MultiStepJob Create(
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        return new MultiStepJob(
            systemName,
            initialState,
            maxAttempts);
    }

    public JobStep AddStep(
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        EnsureTopologyMutable();

        var step = JobStep.Create(
            Id,
            systemName,
            initialState,
            maxAttempts);

        _steps.Add(step);

        return step;
    }

    public void AddDependency(
        JobStep step,
        JobStep dependsOn)
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

        step.AddDependency(dependsOn);
    }

    public void Wait(Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);

        if (IsCancellationRequested)
            throw new JobCancellationRequestedException(Id);

        EnsureStatus(JobStatus.Processing);

        Status = JobStatus.Waiting;
        ClearLease();
    }

    public void Resume()
    {
        EnsureStatus(JobStatus.Waiting);
        Status = JobStatus.Pending;
    }

    private void EnsureTopologyMutable()
    {
        if (Status != JobStatus.Pending || LockedAt is not null)
            throw new InvalidOperationException(
                "Multi-step job topology cannot be " +
                "changed after execution has started.");
    }

    private void EnsureOwnStep(JobStep step)
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

            if (!stepsById.TryGetValue(currentStepId, out var currentStep))
                throw new InvalidOperationException(
                    "Workflow contains a dependency on an unknown job step.");

            foreach (var dependency in currentStep.Dependencies)
                stack.Push(dependency.DependsOnStepId);
        }

        return false;
    }
}
