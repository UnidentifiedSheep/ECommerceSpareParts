using Abstractions.Interfaces;
using Application.Common.Interfaces.Repositories;
using Dapper;
using Domain.CommonEntities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repository;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Persistence.Common;

public class JobRepository<TContext>(
    TContext context,
    IUserContext userContext,
    IQueryableExtensions extensions
) : LinqRepositoryBase<TContext, Job, Guid>(context, extensions), 
    IJobRepository where TContext : DbContext
{
    public async Task<int> TryInsertPendingUniqueAsync(
        IEnumerable<UniqJob> jobs,
        CancellationToken cancellationToken = default)
    {
        var all = jobs.DistinctBy(x => x.NaturalKey).ToList();
        if (all.Count == 0) return 0;
        
        all.ForEach(x => x.Touch(userContext.UserIdOrNull));

        var connection = Context.Database.GetDbConnection();

        var command = new CommandDefinition(
            commandText: """
                INSERT INTO job.jobs (
                    id,
                    job_type,
                    system_name,
                    natural_key,
                    state,
                    status,
                    attempts,
                    max_attempts,
                    error_message,
                    locked_at,
                    lease_expires_at,
                    lease_holder_id,
                    created_at,
                    updated_at
                )
                SELECT
                    x.id,
                    x.job_type,
                    x.system_name,
                    x.natural_key,
                    x.state,
                    x.status,
                    x.attempts,
                    x.max_attempts,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    x.created_at,
                    x.updated_at
                FROM unnest(
                    @Ids,
                    @JobTypes,
                    @SystemNames,
                    @NaturalKeys,
                    @States,
                    @Statuses,
                    @Attempts,
                    @MaxAttempts,
                    @CreatedAts,
                    @UpdatedAts
                ) AS x(
                    id,
                    job_type,
                    system_name,
                    natural_key,
                    state,
                    status,
                    attempts,
                    max_attempts,
                    created_at,
                    updated_at
                )
                ON CONFLICT (system_name, natural_key)
                WHERE status = 'Pending'
                  AND job_type = 'uniq_job'
                DO NOTHING;
                """,
            parameters: new
            {
                Ids = all.Select(x => x.Id).ToArray(),
                JobTypes = all.Select(_ => "uniq_job").ToArray(),
                SystemNames = all.Select(x => x.SystemName).ToArray(),
                NaturalKeys = all.Select(x => x.NaturalKey).ToArray(),
                States = all.Select(x => x.State).ToArray(),
                Statuses = all.Select(_ => "Pending").ToArray(),
                Attempts = all.Select(x => x.Attempts).ToArray(),
                MaxAttempts = all.Select(x => x.MaxAttempts).ToArray(),
                CreatedAts = all.Select(x => x.CreatedAt).ToArray(),
                UpdatedAts = all.Select(x => x.UpdatedAt).ToArray()
            },
            transaction: Context.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }
}