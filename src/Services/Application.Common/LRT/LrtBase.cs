using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities.Job;
using Domain.Exceptions;
using Locan.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class LrtBase<TInputState, TState>(
	IApplicationTransactionFactory transactionService,
	ILogger logger) : ILrtNamedObject<TInputState>
	where TInputState : class, IInputState where TState : class
{
	private Job? _job;
	private IApplicationTransactionContext? _activeTransactionContext;

	private TInputState? _inputState;
	private TState? _state;

	protected IUnitOfWork UnitOfWork => TransactionContext.UnitOfWork;

	protected IApplicationTransactionFactory TransactionFactory => transactionService;

	protected IApplicationTransactionContext TransactionContext => _activeTransactionContext ??
		throw new InvalidOperationException("No LRT transaction is executing.");

	protected IRepository<Job, Guid> JobRepository => TransactionContext.Repositories.Get<Job, Guid>();

	protected ILogger Logger => logger;

	protected IPublishEndpoint Publisher => TransactionContext.Publisher;

	protected CancellationToken CancellationToken { get; private set; }

	protected Job Job => _job ?? throw new InvalidOperationException("Job is not initialized");

	protected TInputState InputState => _inputState ?? throw new InvalidOperationException("LRT input state is not initialized");

	protected TState State => _state ?? throw new InvalidOperationException("LRT state is not initialized");

	protected Guid JobId { get; private set; }

	protected Guid LeaseHolderId { get; private set; }

	protected bool Initialized { get; private set; }

	protected virtual TimeSpan LeaseDuration => TimeSpan.FromMinutes(5);

	public abstract string SystemName { get; }

	public abstract ILocalizableMessage NameLocalizationMessage { get; }
	public abstract ILocalizableMessage DescriptionLocalizationMessage { get; }

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
		_inputState = null;
		_state = null;

		logger.LogInformation("LRT execution started. JobId: {JobId}", JobId);

		while (true)
			try
			{
				if (!Initialized)
				{
					await InitJobAsync();
					await ProcessingJobAsync();
					Initialized = true;
				}

				await DoWork(InputState);
				await SucceedJobAsync();
				logger.LogInformation("LRT execution completed. JobId: {JobId}", JobId);
				break;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				logger.LogInformation("LRT execution cancelled. JobId: {JobId}", JobId);
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
				logger.LogWarning(
					e,
					"LRT stopped because lease was lost. JobId: {JobId}",
					JobId);
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

	protected virtual Task InitJobAsync() => ReloadStateAsync();

	protected void Interrupt(string reason) => throw new LrtInterruptedException(reason);

	protected async Task ReloadStateAsync()
	{
		await GetJobAsync();
		if (string.IsNullOrWhiteSpace(Job.State))
			throw new InvalidOperationException($"LRT '{SystemName}' state is empty.");

		var (input, state) = LrtStateSerializer.Deserialize<TInputState, TState>(Job.State);
		_inputState = input;
		_state = state ?? CreateInitialState(input);
	}

	protected virtual TState CreateInitialState(TInputState inputState) =>
		JsonSerializer.Deserialize<TState>(JsonSerializer.Serialize(inputState)) ??
		throw new InvalidOperationException($"LRT '{SystemName}' state could not be initialized as '{StateType.Name}'.");

	protected async Task SaveStateAsync(TState state)
	{
		var json = LrtStateSerializer.Serialize(InputState, state);
		await ExecuteInTransactionAsync(
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
		if (_activeTransactionContext is null)
		{
			await ExecuteInTransactionAsync(null, (_, _) => GetJobAsync(), CancellationToken);
			return;
		}

		var reload = _job is not null;
		_job = await JobRepository.GetById(JobId, CancellationToken) ??
			throw new InvalidOperationException($"Job with id {JobId} not found");
		if (reload)
			await UnitOfWork.ReloadAsync(_job, CancellationToken);
	}

	protected async Task ExecuteInScopeAsync(
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default)
	{
		await using var transaction = TransactionFactory.Create();
		await action(transaction.Context, cancellationToken);
	}

	protected async Task<TResult> ExecuteInScopeAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		await using var transaction = TransactionFactory.Create();
		return await action(transaction.Context, cancellationToken);
	}

	protected Task ExecuteInTransactionAsync(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default) =>
		ExecuteInTransactionAsync(async (context, ct) =>
		{
			await action(context, ct);
			return true;
		}, settings, cancellationToken);

	protected Task<TResult> ExecuteInTransactionAsync<TResult>(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default) =>
		ExecuteInTransactionAsync(action, settings, cancellationToken);

	private async Task<TResult> ExecuteInTransactionAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		TransactionalAttribute? settings,
		CancellationToken cancellationToken)
	{
		if (_activeTransactionContext is { } active)
			return await action(active, cancellationToken);

		await using var transaction = TransactionFactory.Create();
		var previousState = _state;
		try
		{
			return await transaction.WithSettings(settings).ExecuteAsync(async (context, ct) =>
			{
				_state = previousState;
				_activeTransactionContext = context;
				try
				{
					return await action(context, ct);
				}
				finally
				{
					_activeTransactionContext = null;
				}
			}, cancellationToken);
		}
		catch
		{
			_state = previousState;
			throw;
		}
	}

	protected virtual async Task ProcessingJobAsync()
	{
		await ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			async (context, cancellationToken) =>
			{
				await GetJobAsync();
				Job.Start(LeaseHolderId);
				Job.RenewLease(LeaseHolderId, LeaseDuration);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				logger.LogInformation("LRT job processing started. JobId: {JobId}", JobId);
			},
			CancellationToken);
	}

	protected virtual async Task<bool> AttemptOrFailJobAsync(Exception exception, bool forceFail = false)
	{
		return await ExecuteInTransactionAsync(
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
		await ExecuteInTransactionAsync(
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
		await ExecuteInTransactionAsync(
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
		await ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			async (context, cancellationToken) =>
			{
				await GetJobAsync();
				Job.RenewLease(LeaseHolderId, leaseDuration);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
			},
			CancellationToken);
	}

	protected abstract Task DoWork(TInputState inputState);
}
