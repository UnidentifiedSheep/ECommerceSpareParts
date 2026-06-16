using System.Linq.Expressions;
using Application.Common.Dtos;
using Domain.CommonEntities;

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
}