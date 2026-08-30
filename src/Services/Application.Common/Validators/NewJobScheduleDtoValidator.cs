using Application.Common.Dtos;
using Cronos;
using Domain.CommonEntities.Job;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Validators;

public sealed class NewJobScheduleDtoValidator : AbstractValidator<NewJobScheduleDto>
{
	public NewJobScheduleDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithLocalizationKey("job.schedule.name.required")
			.MaximumLength(JobSchedule.NameMaxLength)
			.WithLocalizationKey("job.schedule.name.max.length");

		RuleFor(x => x.Description)
			.MaximumLength(JobSchedule.DescriptionMaxLength)
			.WithLocalizationKey("job.schedule.description.max.length");

		RuleFor(x => x.JobSystemName).NotEmpty().WithLocalizationKey("job.schedule.job.system.name.required");

		RuleFor(x => x.InputState).NotEmpty().WithLocalizationKey("job.schedule.input.state.required");

		RuleFor(x => x.Cron)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizationKey("job.schedule.cron.required")
			.Must(x => CronExpression.TryParse(x, out _))
			.WithLocalizationKey("job.schedule.cron.invalid");

		RuleFor(x => x.MaxAttempts)
			.GreaterThan(0)
			.WithLocalizationKey("job.max.attempts.must.be.greater.than.zero");
	}
}
