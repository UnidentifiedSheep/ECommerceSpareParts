using System.Linq.Expressions;
using Domain.Interfaces;

namespace Domain.CommonEntities.Job;

public sealed class JobStepDependency :
    Entity<JobStepDependency, JobStepDependencyKey>,
    ILinqEntity<JobStepDependency, JobStepDependencyKey>
{
    private JobStepDependency()
    {
    }

    private JobStepDependency(
        Guid stepId,
        Guid dependsOnStepId)
    {
        StepId = stepId;
        DependsOnStepId = dependsOnStepId;
    }

    public Guid StepId { get; private set; }
    public JobStep Step { get; private set; } = null!;

    public Guid DependsOnStepId { get; private set; }
    public JobStep DependsOnStep { get; private set; } = null!;

    internal static JobStepDependency Create(
        Guid stepId,
        Guid dependsOnStepId)
    {
        if (stepId == dependsOnStepId)
        {
            throw new InvalidOperationException(
                "Job step cannot depend on itself.");
        }

        return new JobStepDependency(
            stepId,
            dependsOnStepId);
    }

    public override JobStepDependencyKey GetId()
    {
        return new JobStepDependencyKey(
            StepId,
            DependsOnStepId);
    }

    public static Expression<Func<JobStepDependency, JobStepDependencyKey>>
        GetKeySelector()
    {
        return x => new JobStepDependencyKey(
            x.StepId,
            x.DependsOnStepId);
    }

    public static Expression<Func<JobStepDependency, bool>>
        GetEqualityExpression(JobStepDependencyKey key)
    {
        return x =>
            x.StepId == key.StepId &&
            x.DependsOnStepId == key.DependsOnStepId;
    }
}

public readonly record struct JobStepDependencyKey(
    Guid StepId,
    Guid DependsOnStepId) : ICompositeKey
{
    public object[] ToArray() => [StepId, DependsOnStepId];
}