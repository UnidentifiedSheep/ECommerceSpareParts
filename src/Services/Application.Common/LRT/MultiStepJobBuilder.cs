using Application.Common.Interfaces.Lrt;
using Domain.CommonEntities.Job;

namespace Application.Common.LRT;

internal sealed class MultiStepJobBuilder(int defaultMaxAttempts, Func<string, string, int, Job> stepFactory)
	: IMultiStepJobBuilder
{
	public Job AddStep(
		string systemName,
		string inputState,
		int? maxAttempts = null) => stepFactory(
		systemName,
		inputState,
		maxAttempts ?? defaultMaxAttempts);

	public void AddDependency(Job job, Job dependsOn)
	{
		var parent = job.MultiStepJob ??
			throw new InvalidOperationException("Job step is not attached to a multi-step job.");
		parent.AddDependency(job, dependsOn);
	}
}
