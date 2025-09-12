using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Mapster;

namespace Api.Common.BackgroundServices;

public class SearchLogBackgroundWorker(ISearchLogger searchLogger, ILogger<SearchLogBackgroundWorker> logger, IServiceProvider serviceProvider)
    : BackgroundService
{
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 1000;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SearchLogBackgroundWorker started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await searchLogger.WaitForTriggerOrTimeoutAsync(_flushInterval, cancellationToken);

                var batch = new List<SearchLogModel>();
                
                while (batch.Count < BatchSize && searchLogger.TryDequeue(out var log))
                    if (log != null)
                        batch.Add(log);
                

                if (batch.Count <= 0) continue;
                
                await SaveBatchAsync(batch, cancellationToken);
                logger.LogInformation("Flushed {Count} search logs.", batch.Count);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("SearchLogBackgroundWorker stopping.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SearchLogBackgroundWorker.");
                await Task.Delay(1000, cancellationToken);
            }
        }

        logger.LogInformation("SearchLogBackgroundWorker stopped.");
    }

    private async Task SaveBatchAsync(List<SearchLogModel> batch, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var dataToInsert = batch.Adapt<List<UserSearchHistory>>();
        await unitOfWork.AddRangeAsync(dataToInsert, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}