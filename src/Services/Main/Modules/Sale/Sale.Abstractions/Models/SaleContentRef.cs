namespace Main.Modules.Sale.Abstractions.Models;

public sealed class SaleContentRef
{
	public required int SaleContentId { get; init; }

	public required int ProductId { get; init; }
}
