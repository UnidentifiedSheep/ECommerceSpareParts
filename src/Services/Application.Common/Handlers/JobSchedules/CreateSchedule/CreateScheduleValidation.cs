using Cronos;
using Domain.CommonEntities;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Handlers.JobSchedules.CreateSchedule;

public class CreateScheduleValidation : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleValidation()
    {
        RuleFor(x => x.NewSchedule.Name)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.name.required")
            .MaximumLength(JobSchedule.NameMaxLength)
            .WithLocalizationKey("job.schedule.name.max.length");

        RuleFor(x => x.NewSchedule.Description)
            .MaximumLength(JobSchedule.DescriptionMaxLength)
            .WithLocalizationKey("job.schedule.description.max.length");

        RuleFor(x => x.NewSchedule.JobSystemName)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.job.system.name.required");

        RuleFor(x => x.NewSchedule.InputState)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.input.state.required");

        RuleFor(x => x.NewSchedule.Cron)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.cron.required")
            .Must(x => CronExpression.TryParse(x, out _))
            .WithLocalizationKey("job.schedule.cron.invalid");

        RuleFor(x => x.NewSchedule.MaxAttempts)
            .GreaterThan(0)
            .WithLocalizationKey("job.max.attempts.must.be.greater.than.zero");
    }
}