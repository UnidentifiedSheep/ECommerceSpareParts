namespace Abstractions.Interfaces.HostedServices;

public interface IBackgroundTaskQueue
{
    void Enqueue(Func<CancellationToken, Task> func);
}