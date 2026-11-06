using FluentValidation;
using FluentValidation.TestHelper;

namespace Tests.Abstractions.Test;

public abstract class ValidationTest<TValidator, TRequest>
	where TValidator : AbstractValidator<TRequest>
{
	protected virtual TValidator CreateValidator() => Activator.CreateInstance<TValidator>();

	protected Task<TestValidationResult<TRequest>> ValidateAsync(TRequest request) =>
		CreateValidator().TestValidateAsync(request, cancellationToken: TestContext.Current.CancellationToken);
}
