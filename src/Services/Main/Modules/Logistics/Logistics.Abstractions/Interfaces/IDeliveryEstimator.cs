using Enums;
using Main.Modules.Logistics.Abstractions.Models;

namespace Main.Modules.Logistics.Abstractions.Interfaces;

public interface IDeliveryEstimator
{
	Task<IReadOnlyDictionary<DateOnly, DeliveryEstimate>> EstimateAsync(
		Supplier supplier,
		string storageCode,
		DateOnly from,
		IEnumerable<DateOnly> supplierDates,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<int, DateTime>> GetArrivalDatesAsync(
		Supplier supplier,
		string storageCode,
		DateTime from,
		IEnumerable<int> deliveryDays,
		CancellationToken cancellationToken = default);
}
