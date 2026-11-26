using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Main.Modules.Logistics.Abstractions.Models;
using Main.Modules.Logistics.Application.Interfaces;

namespace Main.Modules.Logistics.Application.Services;

public sealed class DeliveryEstimator(IDeliveryScheduleCacheRepository cacheRepository) : IDeliveryEstimator
{
	public async Task<IReadOnlyDictionary<DateOnly, DeliveryEstimate>> EstimateAsync(
		Supplier supplier,
		string storageCode,
		DateOnly from,
		IEnumerable<DateOnly> supplierDates,
		CancellationToken cancellationToken = default)
	{
		var calendar = await GetCalendarAsync(supplier, storageCode, cancellationToken);

		return supplierDates
			.Distinct()
			.ToDictionary(x => x, x => Estimate(calendar, from, x));
	}

	public async Task<IReadOnlyDictionary<int, DateTime>> GetArrivalDatesAsync(
		Supplier supplier,
		string storageCode,
		DateTime from,
		IEnumerable<int> deliveryDays,
		CancellationToken cancellationToken = default)
	{
		var calendar = await GetCalendarAsync(supplier, storageCode, cancellationToken);

		return deliveryDays
			.Distinct()
			.ToDictionary(
				x => x,
				x => calendar.GetArrivalAfterDeliveryDays(from, x));
	}

	private async Task<DeliveryCalendar> GetCalendarAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken)
	{
		var schedule = await cacheRepository.GetAsync(supplier, storageCode, cancellationToken);
		return schedule is null
			? DeliveryCalendar.WorkingDays
			: new DeliveryCalendar(schedule.Days, schedule.Exceptions, schedule.ArrivalTime);
	}

	private static DeliveryEstimate Estimate(DeliveryCalendar calendar, DateOnly from, DateOnly supplierDate)
	{
		var date = supplierDate < from ? from : supplierDate;
		var arrival = calendar.GetNextDeliveryDate(date);
		return new DeliveryEstimate(arrival, calendar.CountDeliveryDays(from, arrival));
	}
}
