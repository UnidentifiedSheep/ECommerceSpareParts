using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Extensions;

namespace Main.Api.GraphQl.Logistics.Extensions;

public static class DeliveryDaysGqlExtensions
{
	private static readonly DayOfWeek[] WeekOrder =
	[
		DayOfWeek.Monday,
		DayOfWeek.Tuesday,
		DayOfWeek.Wednesday,
		DayOfWeek.Thursday,
		DayOfWeek.Friday,
		DayOfWeek.Saturday,
		DayOfWeek.Sunday
	];

	public static DeliveryDays ToDeliveryDays(this IEnumerable<DayOfWeek> days)
		=> days.Aggregate(DeliveryDays.None, (result, day) => result | day.ToDeliveryDay());

	public static IReadOnlyList<DayOfWeek> ToDaysOfWeek(this DeliveryDays days)
		=> WeekOrder.Where(day => days.Contains(day)).ToArray();
}
