using System.Threading.Channels;
using Serilog;

namespace MonoliteUnicorn.Services.CacheService;

public class CacheQueue : BackgroundService
{
    private readonly Channel<Func<IServiceProvider, Task>> _queue;
    private readonly IServiceProvider _serviceProvider;

    public CacheQueue(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _queue = Channel.CreateUnbounded<Func<IServiceProvider, Task>>();
    }

    public void Enqueue(Func<IServiceProvider, Task> workItem)
    {
        _queue.Writer.TryWrite(workItem);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await workItem(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в CacheQueue");
            }
        }
    }
}
