using System.Linq.Expressions;
using Application.Common.Dtos;
using Application.Common.Interfaces.Projections;
using Attributes;
using CronExpressionDescriptor;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class JobDtoProjectionProvider
    : ProjectionProviderBase<Job, JobDto>
{
    public override Expression<Func<Job, JobDto>> Projection { get; } =
        job => new JobDto
        {
            Attempts = job.Attempts,
            CreatedAt = job.CreatedAt,
            SystemName = job.SystemName,
            CreatedBy = job.WhoCreated,
            ErrorMessage = job.ErrorMessage,
            MaxAttempts = job.MaxAttempts,
            Id = job.Id,
            LockedAt = job.LockedAt,
            Status = job.Status,
            UpdatedAt = job.UpdatedAt
        };
}

[Lifetime(Lifetime.Scoped)]
public sealed class JobScheduleDtoProjectionProvider(
    IScopedStringLocalizer localizer)
    : ProjectionProviderBase<JobSchedule, JobScheduleDto>
{
    public override Expression<Func<JobSchedule, JobScheduleDto>> Projection { get; } =
        schedule => new JobScheduleDto
        {
            Id = schedule.Id,
            Name = schedule.Name,
            Description = schedule.Description,
            Cron = schedule.Cron,
            LocalizedCron = ExpressionDescriptor.GetDescription(
                schedule.Cron,
                new Options
                {
                    DayOfWeekStartIndexZero = false,
                    Use24HourTimeFormat = true,
                    Locale = localizer.Locale
                }),
            InputState = schedule.InputState,
            LastQueuedAt = schedule.LastQueuedAt,
            MaxAttempts = schedule.MaxAttempts,
            NextRunAt = schedule.NextRunAt,
            JobSystemName = schedule.JobSystemName,
            Enabled = schedule.Enabled
        };
}
