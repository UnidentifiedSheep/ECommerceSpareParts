using Application.Common.Dtos;
using Cronos;
using Domain.CommonEntities.Job;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Validators;

public sealed class PatchJobScheduleDtoValidator : AbstractValidator<PatchJobScheduleDto>
{
    public PatchJobScheduleDtoValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.name.required")
            .MaximumLength(JobSchedule.NameMaxLength)
            .WithLocalizationKey("job.schedule.name.max.length")
            .When(x => x.Name.IsSet);

        RuleFor(x => x.Description.Value)
            .MaximumLength(JobSchedule.DescriptionMaxLength)
            .WithLocalizationKey("job.schedule.description.max.length")
            .When(x => x.Description.IsSet);

        RuleFor(x => x.InputState.Value)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.input.state.required")
            .When(x => x.InputState.IsSet);

        RuleFor(x => x.Cron.Value)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithLocalizationKey("job.schedule.cron.required")
            .Must(x => x is not null && CronExpression.TryParse(x, out _))
            .WithLocalizationKey("job.schedule.cron.invalid")
            .When(x => x.Cron.IsSet);

        RuleFor(x => x.MaxAttempts.Value)
            .GreaterThan(0)
            .WithLocalizationKey("job.max.attempts.must.be.greater.than.zero")
            .When(x => x.MaxAttempts.IsSet);
    }
}
