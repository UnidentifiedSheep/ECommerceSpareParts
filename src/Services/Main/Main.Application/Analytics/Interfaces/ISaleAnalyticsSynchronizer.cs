namespace Main.Application.Analytics.Interfaces;

public interface ISaleAnalyticsSynchronizer
{
	Task SynchronizeAsync(IReadOnlyCollection<Guid> saleIds, CancellationToken cancellationToken);
}
