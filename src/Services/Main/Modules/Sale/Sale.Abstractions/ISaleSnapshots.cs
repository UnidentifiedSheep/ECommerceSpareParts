namespace Main.Modules.Sale.Abstractions;

public interface ISaleSnapshots
{
	Task<SaleSnapshotsPage> GetPageAsync(
		Guid? afterSaleId,
		int take,
		CancellationToken cancellationToken = default);
}

public sealed record SaleSnapshotsPage(
	IReadOnlyList<SaleContentSnapshot> Contents,
	Guid? LastSaleId,
	int SalesCount);
