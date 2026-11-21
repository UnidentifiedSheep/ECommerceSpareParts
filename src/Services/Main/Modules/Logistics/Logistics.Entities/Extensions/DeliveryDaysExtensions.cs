using Main.Entities.Logistics.Enums;

namespace Main.Entities.Logistics.Extensions;

public static class DeliveryDaysExtensions
{
	public static DeliveryDays ToDeliveryDay(this DayOfWeek dayOfWeek)
		=> dayOfWeek switch
		{
			DayOfWeek.Monday => DeliveryDays.Monday,
			DayOfWeek.Tuesday => DeliveryDays.Tuesday,
			DayOfWeek.Wednesday => DeliveryDays.Wednesday,
			DayOfWeek.Thursday => DeliveryDays.Thursday,
			DayOfWeek.Friday => DeliveryDays.Friday,
			DayOfWeek.Saturday => DeliveryDays.Saturday,
			DayOfWeek.Sunday => DeliveryDays.Sunday,
			_ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
		};

	public static bool Contains(this DeliveryDays days, DayOfWeek dayOfWeek)
		=> (days & dayOfWeek.ToDeliveryDay()) != DeliveryDays.None;

	public static int? DaysUntilNext(this DeliveryDays days, DayOfWeek from)
	{
		for (var offset = 0; offset < 7; offset++)
			if (days.Contains((DayOfWeek)(((int)from + offset) % 7)))
				return offset;

		return null;
	}
}
