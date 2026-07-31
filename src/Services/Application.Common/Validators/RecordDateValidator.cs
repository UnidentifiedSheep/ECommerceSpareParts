using Application.Common.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Common.Validators;

public class RecordDateValidator : AbstractValidator<DateTime>
{
    public RecordDateValidator(IOperationDatePolicy datePolicy)
    {
        RuleFor(date => date)
            .Custom((date, context) =>
            {
                var result = datePolicy.IsAllowed(date.ToUniversalTime());
                if (result.IsValid) return;

                context.AddFailure(
                    new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = result.Message
                    });
            });
    }
}
