using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Job;
using Domain.CommonEntities;
using Domain.CommonEnums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class LrtBase(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger logger) : ILrt, ILrtDescriptor
{
    protected IUnitOfWork UnitOfWork => unitOfWork;
    protected IRepository<Job, Guid> JobRepository => jobRepository;
    protected ILogger Logger => logger;
    protected IPublishEndpoint Publisher => publisher;
    protected CancellationToken CancellationToken { get; private set; }
    protected Job Job { get; private set; } = null!;
    protected Guid JobId { get; private set; }
    protected bool Initialized { get; private set; }
    protected abstract IServiceDefinition ServiceDefinition { get; }
    
    public async Task ExecuteAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        JobId = jobId;
        Job = null!;
        Initialized = false;

        logger.LogInformation(
            "LRT execution started. JobId: {JobId}",
            JobId);

        while (true)
        {
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
                await CancelJobAsync();
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
            catch (Exception e)
            {
                if (await AttemptOrFailJobAsync(e))
                {
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
    }

    protected virtual Task InitJobAsync()
    {
        return GetJobAsync();
    }

    protected void Interrupt(string reason)
    {
        throw new LrtInterruptedException(reason);
    }

    protected async Task<T?> GetStateAsync<T>()
    {
        await GetJobAsync();
        return string.IsNullOrWhiteSpace(Job.State) 
            ? default 
            : JsonSerializer.Deserialize<T>(Job.State);
    }

    protected async Task UpdateState<T>(T? state)
    {
        var json = JsonSerializer.Serialize(state);
        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                await GetJobAsync();
                Job.SetState(json);
                await unitOfWork.SaveChangesAsync(CancellationToken);
            },
            CancellationToken);
    }

    protected async Task GetJobAsync()
    {
        Job = await jobRepository.GetById(JobId, CancellationToken)
            ?? throw new InvalidOperationException($"Job with id {JobId} not found");
    }
    
    protected virtual async Task ProcessingJobAsync()
    {
        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                await GetJobAsync();
                Job.Start();
                await PublishStatusUpdatedEvent(Job);
                await unitOfWork.SaveChangesAsync(CancellationToken);
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
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                await GetJobAsync();

                if (Job.CanRetry() && !forceFail)
                {
                    Job.RegisterAttempt();
                    await PublishStatusUpdatedEvent(Job);
                    await unitOfWork.SaveChangesAsync(CancellationToken);
                    return true;
                }

                Job.Fail(exception.Message);
                await PublishStatusUpdatedEvent(Job);
                await unitOfWork.SaveChangesAsync(CancellationToken);
                return false;
            },
            CancellationToken);
    }

    protected virtual async Task SucceedJobAsync()
    {
        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                await GetJobAsync();
                Job.Succeed();
                await PublishStatusUpdatedEvent(Job);
                await unitOfWork.SaveChangesAsync(CancellationToken);
            },
            CancellationToken);
    }

    protected virtual async Task CancelJobAsync()
    {
        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                await GetJobAsync();
                Job.Cancel();
                await PublishStatusUpdatedEvent(Job);
                await unitOfWork.SaveChangesAsync(CancellationToken);
            }, CancellationToken);
    }

    protected async Task PublishStatusUpdatedEvent(Job job)
    {
        await publisher.Publish(
            new JobStatusUpdatedEvent
            {
                JobId = job.Id,
                Status = job.Status.ToString(),
                CurrentAttempt = job.Attempts
            },
            conf => conf.SetRoutingKey(ServiceDefinition.ServiceName),
            CancellationToken);
    }
    
    protected abstract Task DoWork();
    public abstract Type InputType { get; }
    public abstract Type StateType { get; }
}
