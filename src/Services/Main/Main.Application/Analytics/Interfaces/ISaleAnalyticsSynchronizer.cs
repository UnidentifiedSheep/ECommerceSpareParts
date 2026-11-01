using Main.Entities.Sale;

namespace Main.Application.Analytics.Interfaces;

public interface ISaleAnalyticsSynchronizer
{
	Task SynchronizeAsync(IReadOnlyCollection<SaleContent> contents, CancellationToken cancellationToken);
}
