using Application.Common.Interfaces;

namespace Api.Common.HostedServices;

public sealed class StartupTaskHostedService(
    IServiceProvider serviceProvider,
    ILogger<StartupTaskHostedService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var tasks = scope.ServiceProvider.GetServices<IStartupTask>();

        foreach (var task in tasks)
        {
            var taskName = task.GetType().Name;

            try
            {
                logger.LogInformation("Running startup task {TaskName}", taskName);

                await task.ExecuteAsync(cancellationToken);

                logger.LogInformation("Startup task {TaskName} completed", taskName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Startup task {TaskName} failed", taskName);

                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}