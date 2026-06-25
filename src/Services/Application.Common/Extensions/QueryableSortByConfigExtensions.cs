using Abstractions;
using Domain.CommonEntities;
using Domain.CommonEnums;

namespace Application.Common.Extensions;

public static class QueryableSortByConfigExtensions
{
    public static QueryableSortBy ConfigureForJob(this QueryableSortBy sortBy)
    {
        return sortBy.MapDefault<Job, Guid>(x => x.Id)
            .Map<Job, Guid>("id", x => x.Id)
            .Map<Job, DateTime>("createdAt", x => x.CreatedAt)
            .Map<Job, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<Job, JobStatus>("status", x => x.Status)
            .MapDefault<JobSchedule, Guid>(x => x.Id)
            .Map<JobSchedule, Guid>("id", x => x.Id)
            .Map<JobSchedule, DateTime?>("nextRunAt", x => x.NextRunAt)
            .Map<JobSchedule, DateTime?>("lastQueuedAt", x => x.LastQueuedAt)
            .Map<JobSchedule, bool>("enabled", x => x.Enabled);
    }
}