using System.Linq.Expressions;
using Application.Common.Dtos;
using CronExpressionDescriptor;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Projections;

public static class JobProjections
{
    public static readonly Expression<Func<Job, JobDto>> JobProjection =
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

    public static Expression<Func<JobSchedule, JobScheduleDto>> JobScheduleProjection(
        IScopedStringLocalizer localizer)
    {
        return schedule => new JobScheduleDto
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
}