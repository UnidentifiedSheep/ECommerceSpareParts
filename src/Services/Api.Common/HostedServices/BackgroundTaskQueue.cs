using System.Threading.Channels;
using Abstractions.Interfaces.HostedServices;

namespace Api.Common.HostedServices;

public class BackgroundTaskQueue(ILogger<BackgroundTaskQueue> logger) : BackgroundService, IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue =
        Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    public void Enqueue(Func<CancellationToken, Task> workItem)
    {
        if (!_queue.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Не удалось добавить фоновую задачу в очередь.");
        logger.LogInformation("Задание добавлено в очередь фоновых задач.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await workItem(stoppingToken);
                logger.LogInformation("Задание из фоновой задачи выполнено.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при выполнении фоновой задачи.");
            }
        }
    }
}
