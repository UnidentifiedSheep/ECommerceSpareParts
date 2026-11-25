using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Extensions;

namespace Main.Entities.Logistics.Schedule;

public sealed class DeliveryCalendar
{
	private readonly HashSet<DateOnly> _exceptions;

	public DeliveryCalendar(DeliveryDays days, IEnumerable<DateOnly> exceptions)
	{
		Days = days;
		_exceptions = exceptions.ToHashSet();
	}

	public DeliveryDays Days { get; }

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

	public int CountDeliveryDays(DateOnly from, DateOnly to)
	{
		var count = 0;
		for (var date = from.AddDays(1); date <= to; date = date.AddDays(1))
			if (IsDeliveryDay(date))
				count++;

		return count;
	}
}
