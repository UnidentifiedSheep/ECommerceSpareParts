using Application.Common.Models.Lrt;

namespace Application.Common.Interfaces.Lrt;

public interface IMultiStepJobBuilder
{
	LrtStepDefinition AddStep<TInputState>(
		string systemName,
		TInputState inputState,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState;

	LrtStepDefinition AddStep<TInputState>(
		string systemName,
		Func<LrtStepInputContext, TInputState> inputFactory,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState;

	LrtStepDefinition AddStep<TInputState>(
		string systemName,
		Func<LrtStepInputContext, CancellationToken, Task<TInputState>> inputFactory,
		int? maxAttempts = null,
		string? key = null) where TInputState : class, IInputState;

	void AddDependency(LrtStepDefinition step, LrtStepDefinition dependsOn);
}
