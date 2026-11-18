using Main.Modules.Sale.Application.Dtos.Sale;

namespace Main.Modules.Sale.Application.Interfaces.Services;

public interface ISaleBuyCostCalculator
{
	Task<IReadOnlyDictionary<int, decimal>> CalculateAsync(
		IReadOnlyCollection<ProductSaleHistoryDto> sales,
		CancellationToken cancellationToken);
}
