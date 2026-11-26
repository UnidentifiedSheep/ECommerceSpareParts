using Enums;
using FluentAssertions;
using Main.Entities.Logistics.DomainEvents;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;

namespace Tests.Domain.Logistics;

public class DeliveryCalendarTests
{
	private static readonly DateOnly Monday = new(2026, 11, 23);

	private static readonly TimeOnly Arrival = new(14, 0);

	[Fact]
	public void IsDeliveryDay_MatchesScheduledWeekdaysAndSkipsExceptions()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Monday | DeliveryDays.Sunday, [Monday.AddDays(7)], Arrival);

		calendar.IsDeliveryDay(Monday).Should().BeTrue();
		calendar.IsDeliveryDay(Monday.AddDays(1)).Should().BeFalse();
		calendar.IsDeliveryDay(Monday.AddDays(6)).Should().BeTrue();
		calendar.IsDeliveryDay(Monday.AddDays(7)).Should().BeFalse();
	}

	[Fact]
	public void GetNextDeliveryDate_ReturnsSameDayOrNextScheduledDaySkippingExceptions()
	{
		var wednesday = Monday.AddDays(2);
		var calendar = new DeliveryCalendar(DeliveryDays.Wednesday, [wednesday], Arrival);

		calendar.GetNextDeliveryDate(Monday).Should().Be(wednesday.AddDays(7));
		calendar.GetNextDeliveryDate(wednesday.AddDays(7)).Should().Be(wednesday.AddDays(7));
	}

	[Fact]
	public void GetNextDeliveryDate_SkipsConsecutiveExceptionsOfSingleWeeklyDay()
	{
		var calendar = new DeliveryCalendar(
			DeliveryDays.Monday,
			[Monday, Monday.AddDays(7), Monday.AddDays(14)], Arrival);

		calendar.GetNextDeliveryDate(Monday).Should().Be(Monday.AddDays(21));
	}

	[Fact]
	public void GetDateAfterDeliveryDays_ReturnsNthReceivingDayAfterFrom()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, [Monday.AddDays(3)], Arrival);
		var friday = Monday.AddDays(4);

		calendar.GetDateAfterDeliveryDays(friday, 0).Should().Be(friday.AddDays(4));
		calendar.GetDateAfterDeliveryDays(friday, 1).Should().Be(friday.AddDays(4));
		calendar.GetDateAfterDeliveryDays(friday, 2).Should().Be(friday.AddDays(6));
		calendar.GetDateAfterDeliveryDays(Monday, 1).Should().Be(Monday.AddDays(1));
		calendar.GetDateAfterDeliveryDays(Monday, 2).Should().Be(Monday.AddDays(8));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(3)]
	[InlineData(10)]
	public void GetDateAfterDeliveryDays_IsInverseOfCountDeliveryDays(int supplierOffset)
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, [Monday.AddDays(8)], Arrival);

		for (var start = 0; start < 7; start++)
		{
			var from = Monday.AddDays(start);
			var arrival = calendar.GetNextDeliveryDate(from.AddDays(supplierOffset));
			var days = calendar.CountDeliveryDays(from, arrival);

			calendar.GetDateAfterDeliveryDays(from, days).Should().Be(arrival);
		}
	}

	[Fact]
	public void CountDeliveryDays_CountsScheduledDaysAfterFromUpToInclusive()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, [Monday.AddDays(3)], Arrival);

		calendar.CountDeliveryDays(Monday, Monday.AddDays(1)).Should().Be(1);
		calendar.CountDeliveryDays(Monday.AddDays(1), Monday.AddDays(1)).Should().Be(0);
		calendar.CountDeliveryDays(Monday, Monday.AddDays(8)).Should().Be(2);
	}

	[Fact]
	public void GetNextDeliveryDate_FridayToTuesdayOverWeekend()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, [], Arrival);
		var friday = Monday.AddDays(4);

		calendar.GetNextDeliveryDate(friday.AddDays(3)).Should().Be(friday.AddDays(4));
	}

	[Fact]
	public void ScheduleChanges_RaiseSingleKeyedChangedEvent()
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "MAIN-01", DeliveryDays.Monday, Arrival);
		schedule.FlushDomainEvents();

		schedule.SetDays(DeliveryDays.Friday);
		schedule.SetArrivalTime(new TimeOnly(9, 0));
		schedule.AddException(Monday.AddDays(4), Monday);
		schedule.RemoveException(Monday.AddDays(4));

		schedule.FlushDomainEvents()
			.Should().ContainSingle()
			.Which.Should().BeOfType<DeliveryScheduleChangedDomainEvent>()
			.Which.Should().Match<DeliveryScheduleChangedDomainEvent>(x =>
				x.Supplier == Supplier.Armtek && x.StorageCode == "MAIN-01");
	}
}
