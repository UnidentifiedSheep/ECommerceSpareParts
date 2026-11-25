using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Abstractions;
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
		var schedule = await cacheRepository.GetAsync(supplier, storageCode, cancellationToken);
		var calendar = schedule is null ? null : new DeliveryCalendar(schedule.Days, schedule.Exceptions);

		return supplierDates
			.Distinct()
			.ToDictionary(x => x, x => Estimate(calendar, from, x));
	}

	private static DeliveryEstimate Estimate(DeliveryCalendar? calendar, DateOnly from, DateOnly supplierDate)
	{
		var date = supplierDate < from ? from : supplierDate;

		if (calendar is null)
			return new DeliveryEstimate(date, date.DayNumber - from.DayNumber);

		var arrival = calendar.GetNextDeliveryDate(date);
		return new DeliveryEstimate(arrival, calendar.CountDeliveryDays(from, arrival));
	}
}
