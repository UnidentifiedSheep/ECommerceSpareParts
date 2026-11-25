using Enums;
using FluentAssertions;
using Main.Entities.Logistics.DomainEvents;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;

namespace Tests.Domain.Logistics;

public class DeliveryCalendarTests
{
	private static readonly DateOnly Monday = new(2026, 11, 23);

	[Fact]
	public void IsDeliveryDay_MatchesScheduledWeekdaysAndSkipsExceptions()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Monday | DeliveryDays.Sunday, [Monday.AddDays(7)]);

		calendar.IsDeliveryDay(Monday).Should().BeTrue();
		calendar.IsDeliveryDay(Monday.AddDays(1)).Should().BeFalse();
		calendar.IsDeliveryDay(Monday.AddDays(6)).Should().BeTrue();
		calendar.IsDeliveryDay(Monday.AddDays(7)).Should().BeFalse();
	}

	[Fact]
	public void GetNextDeliveryDate_ReturnsSameDayOrNextScheduledDaySkippingExceptions()
	{
		var wednesday = Monday.AddDays(2);
		var calendar = new DeliveryCalendar(DeliveryDays.Wednesday, [wednesday]);

		calendar.GetNextDeliveryDate(Monday).Should().Be(wednesday.AddDays(7));
		calendar.GetNextDeliveryDate(wednesday.AddDays(7)).Should().Be(wednesday.AddDays(7));
	}

	[Fact]
	public void GetNextDeliveryDate_SkipsConsecutiveExceptionsOfSingleWeeklyDay()
	{
		var calendar = new DeliveryCalendar(
			DeliveryDays.Monday,
			[Monday, Monday.AddDays(7), Monday.AddDays(14)]);

		calendar.GetNextDeliveryDate(Monday).Should().Be(Monday.AddDays(21));
	}

	[Fact]
	public void CountDeliveryDays_CountsScheduledDaysAfterFromUpToInclusive()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, [Monday.AddDays(3)]);

		calendar.CountDeliveryDays(Monday, Monday.AddDays(1)).Should().Be(1);
		calendar.CountDeliveryDays(Monday.AddDays(1), Monday.AddDays(1)).Should().Be(0);
		calendar.CountDeliveryDays(Monday, Monday.AddDays(8)).Should().Be(2);
	}

	[Fact]
	public void GetNextDeliveryDate_FridayToTuesdayOverWeekend()
	{
		var calendar = new DeliveryCalendar(DeliveryDays.Tuesday | DeliveryDays.Thursday, []);
		var friday = Monday.AddDays(4);

		calendar.GetNextDeliveryDate(friday.AddDays(3)).Should().Be(friday.AddDays(4));
	}

	[Fact]
	public void ScheduleChanges_RaiseSingleKeyedChangedEvent()
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "MAIN-01", DeliveryDays.Monday);
		schedule.FlushDomainEvents();

		schedule.SetDays(DeliveryDays.Friday);
		schedule.AddException(Monday.AddDays(4), Monday);
		schedule.RemoveException(Monday.AddDays(4));

		schedule.FlushDomainEvents()
			.Should().ContainSingle()
			.Which.Should().BeOfType<DeliveryScheduleChangedDomainEvent>()
			.Which.Should().Match<DeliveryScheduleChangedDomainEvent>(x =>
				x.Supplier == Supplier.Armtek && x.StorageCode == "MAIN-01");
	}
}
