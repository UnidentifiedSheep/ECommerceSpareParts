using System.Text.Json;
using Abstractions.Interfaces.Persistence;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;

namespace Application.Common.LRT;

public abstract class LrtBase(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork) : ILrt, ILrtDescriptor
{
    protected CancellationToken CancellationToken { get; private set; }
    protected Job Job { get; private set; } = null!;
    protected Guid JobId { get; private set; }
    protected bool Initialized { get; private set; }
    
    public async Task ExecuteAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        JobId = jobId;
        Job = null!;
        Initialized = false;

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
                break;
                
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await CancelJobAsync();
                break;
            }
            catch (LrtInterruptedException e)
            {
                await AttemptOrFailJobAsync(e, true);
                break;
            }
            catch (Exception e)
            {
                if (await AttemptOrFailJobAsync(e))
                    continue;
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
                await unitOfWork.SaveChangesAsync(CancellationToken);
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
                    await unitOfWork.SaveChangesAsync(CancellationToken);
                    return true;
                }

                Job.Fail(exception.Message);
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
                await unitOfWork.SaveChangesAsync(CancellationToken);
            }, CancellationToken);
    }
    
    protected abstract Task DoWork();
    public abstract Type InputType { get; }
    public abstract Type StateType { get; }
}
