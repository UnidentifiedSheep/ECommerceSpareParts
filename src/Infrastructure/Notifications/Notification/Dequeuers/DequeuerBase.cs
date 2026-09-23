using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NamedObject.Core.Interfaces;
using Notification.Options;

namespace Notification.Dequeuers;

public abstract class DequeuerBase(
	ILogger logger,
	IOptions<ChannelOptionBase> options
	) : BackgroundService, INamedObject
{
	private readonly SemaphoreSlim _wakeUp = new(0, 1);

	public abstract string SystemName { get; }

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

			await _wakeUp.WaitAsync(options.Value.DelayBeforeBatch, stoppingToken);
		}
	}

	/// <returns>True when another batch should be attempted without waiting.</returns>
	public abstract Task<bool> DequeueAsync(CancellationToken cancellationToken);

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
