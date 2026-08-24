using Dapper;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Persistence.Common.Jobs;

internal sealed class PendingUniqueJobFilter<TContext>(TContext context)
    where TContext : DbContext
{
    public async Task<IReadOnlyList<Job>> FilterAsync(
        IReadOnlyCollection<Job> jobs,
        CancellationToken cancellationToken)
    {
        var uniqueJobs = jobs
            .Where(x => x.NaturalKey is not null)
            .ToList();

        if (uniqueJobs.Count == 0)
            return jobs.ToList();

        var existingKeys = await LockAndGetExistingKeysAsync(
            uniqueJobs,
            cancellationToken);

        return jobs
            .Where(x =>
                x.NaturalKey is null ||
                !existingKeys.Contains(new JobKey(
                    x.SystemName,
                    x.NaturalKey)))
            .ToList();
    }

    private async Task<HashSet<JobKey>> LockAndGetExistingKeysAsync(
        IReadOnlyCollection<Job> jobs,
        CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            commandText: """
                SELECT pg_advisory_xact_lock(
                    hashtextextended(
                        i.system_name || chr(31) || i.natural_key,
                        0))
                FROM (
                    SELECT x.system_name, x.natural_key
                    FROM unnest(
                        @SystemNames,
                        @NaturalKeys
                    ) AS x(system_name, natural_key)
                    ORDER BY x.system_name, x.natural_key
                ) AS i;

                SELECT
                    x.system_name AS "SystemName",
                    x.natural_key AS "NaturalKey"
                FROM unnest(
                    @SystemNames,
                    @NaturalKeys
                ) AS x(system_name, natural_key)
                INNER JOIN job.jobs AS j
                    ON j.system_name = x.system_name
                   AND j.natural_key = x.natural_key
                WHERE j.status = 'Pending';
                """,
            parameters: new
            {
                SystemNames = jobs.Select(x => x.SystemName).ToArray(),
                NaturalKeys = jobs.Select(x => x.NaturalKey!).ToArray()
            },
            transaction: context.Database.CurrentTransaction!
                .GetDbTransaction(),
            cancellationToken: cancellationToken);

        var connection = context.Database.GetDbConnection();
        await using var result = await connection.QueryMultipleAsync(command);
        await result.ReadAsync();
        return (await result.ReadAsync<JobKey>()).ToHashSet();
    }

    private sealed record JobKey(
        string SystemName,
        string NaturalKey);
}
