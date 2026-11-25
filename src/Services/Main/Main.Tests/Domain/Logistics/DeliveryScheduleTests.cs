using Enums;
using Exceptions;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Schedule;

namespace Tests.Domain.Logistics;

public class DeliveryScheduleTests
{
	private static readonly DateOnly Monday = new(2026, 11, 23);

	[Fact]
	public void Create_TrimsStorageCodeAndUsesSupplierAndStorageAsKey()
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "  MAIN-01 ", DeliveryDays.Monday | DeliveryDays.Thursday);

		schedule.StorageCode.Should().Be("MAIN-01");
		schedule.GetId().Should().Be(new DeliveryScheduleKey(Supplier.Armtek, "MAIN-01"));
		schedule.Days.Should().Be(DeliveryDays.Monday | DeliveryDays.Thursday);
		schedule.Exceptions.Should().BeEmpty();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Create_RejectsEmptyStorageCode(string storageCode)
	{
		var action = () => DeliverySchedule.Create(Supplier.Tmtr, storageCode, DeliveryDays.Monday);

		action.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void Create_RejectsTooLongStorageCode()
	{
		var action = () => DeliverySchedule.Create(
			Supplier.Tmtr,
			new string('A', DeliverySchedule.StorageCodeMaxLength + 1),
			DeliveryDays.Monday);

		action.Should().Throw<InvalidInputException>();
	}

	[Theory]
	[InlineData(DeliveryDays.None)]
	[InlineData((DeliveryDays)128)]
	[InlineData(DeliveryDays.Monday | (DeliveryDays)256)]
	public void SetDays_RejectsEmptyOrUnsupportedDays(DeliveryDays days)
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "MAIN-01", DeliveryDays.Monday);

		var action = () => schedule.SetDays(days);

		action.Should().Throw<InvalidInputException>();
		schedule.Days.Should().Be(DeliveryDays.Monday);
	}

	[Fact]
	public void AddException_CreatesExceptionBoundToSchedule()
	{
		var schedule = DeliverySchedule.Create(Supplier.FavoritParts, "MAIN-01", DeliveryDays.All);

		var exception = schedule.AddException(Monday, Monday);

		exception.GetId().Should().Be(new ScheduleExceptionKey(Supplier.FavoritParts, "MAIN-01", Monday));
		schedule.Exceptions.Should().ContainSingle().Which.Should().BeSameAs(exception);
	}

	[Fact]
	public void AddException_RejectsPastAndDuplicateDates()
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "MAIN-01", DeliveryDays.All);
		schedule.AddException(Monday, Monday);

		var past = () => schedule.AddException(Monday.AddDays(-1), Monday);
		var duplicate = () => schedule.AddException(Monday, Monday);

		past.Should().Throw<InvalidInputException>();
		duplicate.Should().Throw<ScheduleExceptionAlreadyExistsException>();
		schedule.Exceptions.Should().ContainSingle();
	}

	[Fact]
	public void RemoveException_RemovesExistingAndRejectsMissingDate()
	{
		var schedule = DeliverySchedule.Create(Supplier.Armtek, "MAIN-01", DeliveryDays.All);
		schedule.AddException(Monday, Monday);

		schedule.RemoveException(Monday);
		var missing = () => schedule.RemoveException(Monday);

		schedule.Exceptions.Should().BeEmpty();
		missing.Should().Throw<ScheduleExceptionNotFoundException>();
	}
}
