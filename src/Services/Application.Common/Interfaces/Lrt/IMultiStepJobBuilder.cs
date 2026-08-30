using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Lrt;

public interface IMultiStepJobBuilder
{
	Job AddStep(
		string systemName,
		string inputState,
		int maxAttempts = 3);

	void AddDependency(Job job, Job dependsOn);
}
