using Cronos;
using Domain.CommonEntities;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Handlers.JobSchedules.UpdateSchedule;

public class UpdateScheduleValidation : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleValidation()
    {
        RuleFor(x => x.Patch.Name.Value)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.name.required")
            .MaximumLength(JobSchedule.NameMaxLength)
            .WithLocalizationKey("job.schedule.name.max.length")
            .When(x => x.Patch.Name.IsSet);

        RuleFor(x => x.Patch.Description.Value)
            .MaximumLength(JobSchedule.DescriptionMaxLength)
            .WithLocalizationKey("job.schedule.description.max.length")
            .When(x => x.Patch.Description.IsSet);

        RuleFor(x => x.Patch.InputState.Value)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.input.state.required")
            .When(x => x.Patch.InputState.IsSet);

        RuleFor(x => x.Patch.Cron.Value)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.cron.required")
            .Must(x => x is not null && CronExpression.TryParse(x, out _))
            .WithLocalizationKey("job.schedule.cron.invalid")
            .When(x => x.Patch.Cron.IsSet);

        RuleFor(x => x.Patch.MaxAttempts.Value)
            .GreaterThan(0)
            .WithLocalizationKey("job.max.attempts.must.be.greater.than.zero")
            .When(x => x.Patch.MaxAttempts.IsSet);
    }
}