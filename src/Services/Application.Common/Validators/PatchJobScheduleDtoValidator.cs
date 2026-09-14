using Application.Common.Dtos;
using Application.Common.Extensions;
using Cronos;
using Domain;
using Domain.CommonEntities.Job;
using FluentValidation;

namespace Application.Common.Validators;

public sealed class PatchJobScheduleDtoValidator : AbstractValidator<PatchJobScheduleDto>
{
	public PatchJobScheduleDtoValidator()
	{
		RuleFor(x => x.Name.Value)
			.NotEmpty()
			.WithLocalizableError(JobScheduleNameRequiredMessage.Instance)
			.MaximumLength(JobSchedule.NameMaxLength)
			.WithLocalizableError(JobScheduleNameMaxLengthMessage.Instance)
			.When(x => x.Name.IsSet);

		RuleFor(x => x.Description.Value)
			.MaximumLength(JobSchedule.DescriptionMaxLength)
			.WithLocalizableError(JobScheduleDescriptionMaxLengthMessage.Instance)
			.When(x => x.Description.IsSet);

		RuleFor(x => x.InputState.Value)
			.NotEmpty()
			.WithLocalizableError(JobScheduleInputStateRequiredMessage.Instance)
			.When(x => x.InputState.IsSet);

		RuleFor(x => x.Cron.Value)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(JobScheduleCronRequiredMessage.Instance)
			.Must(x => x is not null && CronExpression.TryParse(x, out _))
			.WithLocalizableError(JobScheduleCronInvalidMessage.Instance)
			.When(x => x.Cron.IsSet);

		RuleFor(x => x.MaxAttempts.Value)
			.GreaterThan(0)
			.WithLocalizableError(JobMaxAttemptsMustBeGreaterThanZeroMessage.Instance)
			.When(x => x.MaxAttempts.IsSet);
	}
}
