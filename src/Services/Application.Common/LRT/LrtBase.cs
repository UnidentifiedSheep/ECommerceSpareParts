using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Domain.Exceptions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class LrtBase<TInputState, TState>(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger logger
) : ILrtNamedObject<TInputState>
    where TInputState : class, IInputState
    where TState : class, TInputState
{
    protected IUnitOfWork UnitOfWork => unitOfWork;
    protected IApplicationTransactionService TransactionService => transactionService;
    protected IRepository<Job, Guid> JobRepository => jobRepository;
    protected ILogger Logger => logger;
    protected IPublishEndpoint Publisher => publisher;
    protected CancellationToken CancellationToken { get; private set; }
    private Job? _job;
    private TState? _state;
    protected Job Job => _job ?? throw new InvalidOperationException("Job is not initialized");
    protected TState State => _state ??
                              throw new InvalidOperationException("LRT state is not initialized");
    protected Guid JobId { get; private set; }
    protected Guid LeaseHolderId { get; private set; }
    protected bool Initialized { get; private set; }
    protected virtual TimeSpan LeaseDuration => TimeSpan.FromMinutes(5);
    public abstract string SystemName { get; }
    public abstract string NameLocalizationKey { get; }
    public abstract string DescriptionLocalizationKey { get; }
    public Type InputType => typeof(TInputState);
    public Type StateType => typeof(TState);

    public async Task ExecuteAsync(
        Guid jobId,
        Guid leaseHolderId,
        CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        JobId = jobId;
        LeaseHolderId = leaseHolderId;
        Initialized = false;
        _job = null;
        _state = null;

        logger.LogInformation(
            "LRT execution started. JobId: {JobId}",
            JobId);

        while (true)
            try
            {
                if (!Initialized)
                {
                    await InitJobAsync();
                    await ProcessingJobAsync();
                    Initialized = true;
                }

                await DoWork();
                await SucceedJobAsync();
                logger.LogInformation(
                    "LRT execution completed. JobId: {JobId}",
                    JobId);
                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation(
                    "LRT execution cancelled. JobId: {JobId}",
                    JobId);
                break;
            }
            catch (LrtInterruptedException e)
            {
                await AttemptOrFailJobAsync(e, true);
                logger.LogWarning(
                    e,
                    "LRT execution interrupted. JobId: {JobId}",
                    JobId);
                break;
            }
            catch (JobLeaseLostException e)
            {
                logger.LogWarning(e, "LRT stopped because lease was lost. JobId: {JobId}", JobId);
                break;
            }
            catch (JobCancellationRequestedException e)
            {
                await CancelJobAsync();
                logger.LogInformation(
                    e,
                    "LRT execution cancelled by request. JobId: {JobId}",
                    JobId);

                break;
            }
            catch (Exception e)
            {
                if (await AttemptOrFailJobAsync(e))
                {
                    await ReloadStateAsync();
                    logger.LogWarning(
                        e,
                        "LRT execution attempt failed. JobId: {JobId}, Attempts: {Attempts}/{MaxAttempts}",
                        JobId,
                        Job.Attempts,
                        Job.MaxAttempts);
                    continue;
                }

                logger.LogError(
                    e,
                    "LRT execution failed. JobId: {JobId}, Attempts: {Attempts}/{MaxAttempts}",
                    JobId,
                    Job.Attempts,
                    Job.MaxAttempts);
                break;
            }
    }

    protected virtual Task InitJobAsync() { return ReloadStateAsync(); }

    protected void Interrupt(string reason) { throw new LrtInterruptedException(reason); }

    protected async Task ReloadStateAsync()
    {
        await GetJobAsync();
        _state = string.IsNullOrWhiteSpace(Job.State)
            ? throw new InvalidOperationException(
                $"LRT '{SystemName}' state is empty.")
            : JsonSerializer.Deserialize<TState>(Job.State)
              ?? throw new InvalidOperationException(
                  $"LRT '{SystemName}' state could not be deserialized as '{StateType.Name}'.");
    }

    protected async Task SaveStateAsync(TState state)
    {
        var json = JsonSerializer.Serialize(state);
        await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();
                Job.SetState(json, LeaseHolderId);
                Job.RenewLease(LeaseHolderId, LeaseDuration);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
            },
            CancellationToken);

        _state = state;
    }

    protected async Task GetJobAsync()
    {
        if (_job != null)
            await unitOfWork.ReloadAsync(_job, CancellationToken);
        else
            _job = await jobRepository.GetById(JobId, CancellationToken)
                   ?? throw new InvalidOperationException($"Job with id {JobId} not found");
    }

    protected virtual async Task ProcessingJobAsync()
    {
        await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();
                Job.Start(LeaseHolderId);
                Job.RenewLease(LeaseHolderId, LeaseDuration);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "LRT job processing started. JobId: {JobId}",
                    JobId);
            },
            CancellationToken);
    }

    protected virtual async Task<bool> AttemptOrFailJobAsync(
        Exception exception,
        bool forceFail = false)
    {
        return await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();

                if (Job.CanRetry() && !forceFail)
                {
                    Job.RegisterAttempt(LeaseHolderId);
                    await context.UnitOfWork.SaveChangesAsync(cancellationToken);
                    return true;
                }

                Job.Fail(LeaseHolderId, exception.Message);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
                return false;
            },
            CancellationToken);
    }

    protected virtual async Task SucceedJobAsync()
    {
        await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();
                Job.Succeed(LeaseHolderId);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
            },
            CancellationToken);
    }

    protected virtual async Task CancelJobAsync()
    {
        await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();
                Job.Cancel(LeaseHolderId);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
            },
            CancellationToken);
    }

    protected async Task RenewLeaseAsync(TimeSpan leaseDuration)
    {
        await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                await GetJobAsync();
                Job.RenewLease(LeaseHolderId, leaseDuration);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
            },
            CancellationToken);
    }

    protected abstract Task DoWork();
}
