using Abstractions.Interfaces.Persistence;
using System.Text.Json;
using Attributes;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NamedObject.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Interfaces.Recipient;
using Notification.Core.Interfaces.Repositories;
using Notification.Options;
using DeliveryEntity = Notification.Core.Entities.NotificationDelivery;
using NotificationRequest = Notification.Core.NotificationDelivery;
using SendResult = Notification.Core.SendResult;

namespace Notification.Dequeuers;

public class Dequeuer<TRecipient>(
	string systemName,
	ILogger<Dequeuer<TRecipient>> logger,
	ChannelOptionsBase options,
	IServiceScopeFactory scopeFactory
	) : BackgroundService, INamedObject
	where TRecipient : INotificationRecipient
{
	private readonly SemaphoreSlim _wakeUp = new(0, 1);

	public string SystemName { get; } = !string.IsNullOrWhiteSpace(systemName)
		? systemName.Trim()
		: throw new ArgumentException("Channel system name must not be empty.", nameof(systemName));

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (await DequeueAsync(stoppingToken))
					continue;
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				logger.LogError(
					exception,
					"Error dequeuing notifications for channel {ChannelSystemName}.",
					SystemName);
			}

			await _wakeUp.WaitAsync(options.DelayBeforeBatch, stoppingToken);
		}
	}

	/// <returns>True when another batch should be attempted without waiting.</returns>
	private async Task<bool> DequeueAsync(CancellationToken cancellationToken)
	{
		await using var scope = scopeFactory.CreateAsyncScope();
		var provider = scope.ServiceProvider;
		var repository = provider.GetRequiredService<INotificationDeliveryRepository>();
		var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
		var recipientResolver = provider.GetRequiredService<IRecipientResolver>();
		var definitions = provider.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>();
		var channel = provider
			.GetRequiredService<INamedObjectRegistry<INotificationChannel>>()
			.GetBySystemName(SystemName);

		var (hasNext, processingError) = await unitOfWork.ExecuteWithTransaction(
			TransactionalAttribute.ReadCommitted(0, 0),
			async () =>
			{
				var batch = await repository
					.GetPendingBatchForUpdateAsync(
						SystemName,
						options.BatchSize,
						cancellationToken);

				if (batch.Count == 0) return (HasNext: false, Error: null);

				var userIdsToResolve = batch
					.Where(delivery => delivery.RecipientJson is null)
					.Select(delivery => delivery.Notification.UserId)
					.Distinct()
					.ToArray();
				var recipientsByUser = userIdsToResolve.Length == 0
					? new Dictionary<Guid, TRecipient>()
					: await recipientResolver.ResolveAsync<TRecipient>(userIdsToResolve, cancellationToken);

				var error = await ProcessAsync(
					batch,
					recipientsByUser,
					definitions,
					channel,
					cancellationToken);

				await unitOfWork.SaveChangesAsync(cancellationToken);

				return (HasNext: await repository.HasNextAsync(SystemName, cancellationToken), Error: error);
			},
			cancellationToken);

		if (processingError is not null)
			ExceptionDispatchInfo.Capture(processingError).Throw();

		return hasNext;
	}

	private async Task<Exception?> ProcessAsync(
		IReadOnlyList<DeliveryEntity> deliveries,
		IReadOnlyDictionary<Guid, TRecipient> recipients,
		INamedObjectRegistry<INotificationDefinition> definitions,
		INotificationChannel channel,
		CancellationToken cancellationToken)
	{
		var requests = new List<NotificationRequest>();
		var owners = new List<DeliveryEntity>();
		Exception? processingError = null;

		foreach (var delivery in deliveries)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				var notification = definitions
					.GetBySystemName(delivery.Notification.NotificationSystemName)
					.FromEntity(delivery.Notification);
				var destination = delivery.RecipientJson != null
					? JsonSerializer.Deserialize<INotificationRecipient>(delivery.RecipientJson)
					: recipients.GetValueOrDefault(delivery.Notification.UserId);

				if (destination is not null && destination.ChannelSystemName != SystemName)
					throw new InvalidOperationException("Stored recipient does not match delivery channel.");

				if (destination is null)
				{
					RecordFailure(delivery, $"No recipient found for channel '{SystemName}'.");
					continue;
				}

				requests.Add(new NotificationRequest(notification, destination));
				owners.Add(delivery);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception exception)
			{
				processingError ??= exception;
				logger.LogError(exception,
					"Unable to prepare notification {NotificationId} for channel {ChannelSystemName}.",
					delivery.NotificationId, SystemName);
				RecordFailure(delivery, exception.Message);
			}
		}

		if (requests.Count == 0) return processingError;

		IReadOnlyList<SendResult> results;
		try
		{
			results = await channel.SendBatchAsync(requests, cancellationToken);
			if (results.Count != requests.Count)
				throw new InvalidOperationException(
					$"Channel '{SystemName}' returned {results.Count} results for {requests.Count} deliveries.");
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			processingError ??= exception;
			logger.LogError(
				exception,
				"Channel {ChannelSystemName} failed to send a batch.",
				SystemName);
			foreach (var delivery in owners.Distinct())
				RecordFailure(delivery, exception.Message);
			return processingError;
		}

		foreach (var group in owners
			         .Select((delivery, index) => (delivery, result: results[index]))
			         .GroupBy(item => item.delivery))
		{
			var failure = group.FirstOrDefault(item => !item.result.Succeeded).result;
			if (failure is null)
				group.Key.MarkDelivered();
			else
				RecordFailure(group.Key, failure.Error);
		}

		return processingError;
	}

	private void RecordFailure(DeliveryEntity delivery, string? error)
	{
		delivery.Fail(
			string.IsNullOrWhiteSpace(error) ? "Notification delivery failed." : error,
			options.MaxAttempts);
	}

	public void SkipDelay()
	{
		try
		{
			_wakeUp.Release();
		}
		catch (SemaphoreFullException)
		{
			// A wake-up is already pending.
		}
	}
}
