using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using Persistence.Common.Jobs;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Persistence.Common;

internal sealed class JobRepository<TContext>(
    TContext context,
    IQueryableExtensions extensions,
    PendingUniqueJobFilter<TContext> uniqueJobFilter
) : LinqRepositoryBase<TContext, Job, Guid>(context, extensions),
    IJobRepository where TContext : DbContext
{
    public async Task<IReadOnlyList<Guid>> InsertJobsAsync(
        IEnumerable<Job> jobs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        var all = Normalize(jobs);

        EnsureValid(all);
        if (all.Count == 0) return [];

        if (Context.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "Jobs must be inserted inside a database transaction.");

        var insertable = await uniqueJobFilter.FilterAsync(
            all,
            cancellationToken);
        if (insertable.Count == 0) return [];

        await Context.AddRangeAsync(insertable, cancellationToken);
        return insertable.Select(x => x.Id).ToList();
    }

    private static List<Job> Normalize(IEnumerable<Job> jobs)
    {
        var result = new List<Job>();
        var ids = new HashSet<Guid>();
        var uniqueKeys = new HashSet<JobKey>();

        foreach (var job in jobs)
        {
            ArgumentNullException.ThrowIfNull(job);

            if (!ids.Add(job.Id))
                continue;

            if (job.NaturalKey is not null &&
                !uniqueKeys.Add(new JobKey(
                    job.SystemName,
                    job.NaturalKey)))
                continue;

            result.Add(job);
        }

        return result;
    }

    private static void EnsureValid(IReadOnlyCollection<Job> jobs)
    {
        if (jobs.Any(x => x is not SingleRunJob and not MultiStepJob))
            throw new InvalidOperationException(
                "Unsupported job type in job batch.");
    }

    private readonly record struct JobKey(
        string SystemName,
        string NaturalKey);
}
