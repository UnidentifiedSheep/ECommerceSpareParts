using Application.Common.Dtos;
using Application.Common.Extensions;
using Cronos;
using Domain;
using Domain.CommonEntities.Job;
using FluentValidation;

namespace Application.Common.Validators;

public sealed class NewJobScheduleDtoValidator : AbstractValidator<NewJobScheduleDto>
{
	public NewJobScheduleDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithLocalizableError(JobScheduleNameRequiredMessage.Instance)
			.MaximumLength(JobSchedule.NameMaxLength)
			.WithLocalizableError(JobScheduleNameMaxLengthMessage.Instance);

		RuleFor(x => x.Description)
			.MaximumLength(JobSchedule.DescriptionMaxLength)
			.WithLocalizableError(JobScheduleDescriptionMaxLengthMessage.Instance);

		RuleFor(x => x.JobSystemName)
			.NotEmpty()
			.WithLocalizableError(JobScheduleJobSystemNameRequiredMessage.Instance);

		RuleFor(x => x.InputState)
			.NotEmpty()
			.WithLocalizableError(JobScheduleInputStateRequiredMessage.Instance);

		RuleFor(x => x.Cron)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(JobScheduleCronRequiredMessage.Instance)
			.Must(x => CronExpression.TryParse(x, out _))
			.WithLocalizableError(JobScheduleCronInvalidMessage.Instance);

		RuleFor(x => x.MaxAttempts)
			.GreaterThan(0)
			.WithLocalizableError(JobMaxAttemptsMustBeGreaterThanZeroMessage.Instance);
	}
}
