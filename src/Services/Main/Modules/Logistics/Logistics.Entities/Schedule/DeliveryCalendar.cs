using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Extensions;

namespace Main.Entities.Logistics.Schedule;

public sealed class DeliveryCalendar
{
	private readonly HashSet<DateOnly> _exceptions;

	public DeliveryCalendar(DeliveryDays days, IEnumerable<DateOnly> exceptions, TimeOnly arrivalTime)
	{
		Days = days;
		ArrivalTime = arrivalTime;
		_exceptions = exceptions.ToHashSet();
	}

	public static TimeOnly EndOfDay { get; } = new(23, 59, 59);

	public static DeliveryCalendar WorkingDays { get; } = new(
		DeliveryDays.Monday |
		DeliveryDays.Tuesday |
		DeliveryDays.Wednesday |
		DeliveryDays.Thursday |
		DeliveryDays.Friday,
		[],
		EndOfDay);

	public DeliveryDays Days { get; }

	public TimeOnly ArrivalTime { get; }

	public bool IsDeliveryDay(DateOnly date)
		=> Days.Contains(date.DayOfWeek) && !_exceptions.Contains(date);

	public DateOnly GetNextDeliveryDate(DateOnly from)
	{
		var date = from;
		while (true)
		{
			var offset = Days.DaysUntilNext(date.DayOfWeek) ??
				throw new InvalidOperationException("Delivery calendar has no delivery days.");
			date = date.AddDays(offset);
			if (!_exceptions.Contains(date))
				return date;

			date = date.AddDays(1);
		}
	}

	public DateOnly GetDateAfterDeliveryDays(DateOnly from, int deliveryDays)
	{
		if (deliveryDays <= 0)
			return GetNextDeliveryDate(from);

		var date = from;
		for (var i = 0; i < deliveryDays; i++)
			date = GetNextDeliveryDate(date.AddDays(1));

		return date;
	}

	public DateTime GetArrivalAfterDeliveryDays(DateTime from, int deliveryDays)
	{
		var date = GetDateAfterDeliveryDays(DateOnly.FromDateTime(from), deliveryDays);
		var arrival = date.ToDateTime(ArrivalTime, DateTimeKind.Utc);

		return arrival > from
			? arrival
			: GetNextDeliveryDate(date.AddDays(1)).ToDateTime(ArrivalTime, DateTimeKind.Utc);
	}

	public int CountDeliveryDays(DateOnly from, DateOnly to)
	{
		var count = 0;
		for (var date = from.AddDays(1); date <= to; date = date.AddDays(1))
			if (IsDeliveryDay(date))
				count++;

		return count;
	}
}
