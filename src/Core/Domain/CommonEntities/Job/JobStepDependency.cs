using System.Linq.Expressions;
using Domain.Interfaces;

namespace Domain.CommonEntities.Job;

public sealed class JobStepDependency : Entity<JobStepDependency, JobStepDependencyKey>,
	ILinqEntity<JobStepDependency, JobStepDependencyKey>
{
	private JobStepDependency()
	{
	}

	private JobStepDependency(
		MultiStepJob multiStepJob,
		Job step,
		Job dependsOnStep)
	{
		MultiStepJobId = multiStepJob.Id;
		MultiStepJob = multiStepJob;
		StepId = step.Id;
		Step = step;
		DependsOnStepId = dependsOnStep.Id;
		DependsOnStep = dependsOnStep;
	}

	public Guid MultiStepJobId { get; private set; }

	public MultiStepJob MultiStepJob { get; private set; } = null!;

	public Guid StepId { get; }

	public Job Step { get; private set; } = null!;

	public Guid DependsOnStepId { get; }

	public Job DependsOnStep { get; private set; } = null!;

	public static Expression<Func<JobStepDependency, JobStepDependencyKey>> GetKeySelector() => x =>
		new JobStepDependencyKey(x.StepId, x.DependsOnStepId);

	public static Expression<Func<JobStepDependency, bool>> GetEqualityExpression(JobStepDependencyKey key)
	{
		return x => x.StepId == key.StepId && x.DependsOnStepId == key.DependsOnStepId;
	}

	internal static JobStepDependency Create(
		MultiStepJob multiStepJob,
		Job step,
		Job dependsOnStep)
	{
		ArgumentNullException.ThrowIfNull(multiStepJob);
		ArgumentNullException.ThrowIfNull(step);
		ArgumentNullException.ThrowIfNull(dependsOnStep);

		if (step.Id == dependsOnStep.Id)
			throw new InvalidOperationException("Job step cannot depend on itself.");

		return new JobStepDependency(
			multiStepJob,
			step,
			dependsOnStep);
	}

	public override JobStepDependencyKey GetId() => new(StepId, DependsOnStepId);
}

public readonly record struct JobStepDependencyKey(Guid StepId, Guid DependsOnStepId) : ICompositeKey
{
	public object[] ToArray() => [StepId, DependsOnStepId];
}
