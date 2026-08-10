using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Interfaces.ServiceProvider;

namespace Tests.Abstractions.Test;

public abstract class LrtIntegrationTestBase<
    TLrt,
    TSp,
    TArgs,
    TContext>
    : IntegrationTestBase<TSp, TArgs, TContext>
    where TLrt : class, ILrtNamedObject
    where TSp : IServiceProviderBuilder<TArgs>, new()
    where TArgs : IServiceProviderArgument
    where TContext : DbContext
{
    protected async Task<LrtExecutionResult> ExecuteLrt(
        string inputState = NoneInputState.Json,
        int maxAttempts = 3,
        CancellationToken cancellationToken = default)
    {
        var lrt = ActivatorUtilities.CreateInstance<TLrt>(
            Scope.ServiceProvider);
        var leaseHolderId = Guid.NewGuid();
        var job = lrt is IMultiStepLrt
            ? Scope.ServiceProvider
                .GetRequiredService<IJobCreationDispatcher>()
                .Create(lrt.SystemName, inputState, maxAttempts)
            : SingleRunJob.Create(
                lrt.SystemName,
                lrt.ValidateState(inputState),
                maxAttempts);
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        await Context.AddAsync(job, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        await lrt.ExecuteAsync(
            job.Id,
            leaseHolderId,
            cancellationToken);

        using var verificationScope = Sp.CreateScope();
        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<TContext>();
        var persistedJob = await verificationContext
            .Set<Job>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == job.Id, cancellationToken);

        return new LrtExecutionResult(persistedJob);
    }
}

public sealed record LrtExecutionResult(Job Job)
{
    public TState GetState<TState>() where TState : class
    {
        return JsonSerializer.Deserialize<TState>(Job.State)
               ?? throw new InvalidOperationException(
                   $"LRT state could not be deserialized as '{typeof(TState).Name}'.");
    }
}
