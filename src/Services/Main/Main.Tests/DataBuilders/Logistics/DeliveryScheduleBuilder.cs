using Bogus;
using Enums;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using Tests.Abstractions;

namespace Tests.DataBuilders.Logistics;

public sealed class DeliveryScheduleBuilder(Faker faker) : BuilderBase<DeliverySchedule>(faker)
{
	private readonly List<DateOnly> _exceptions = [];

	public Supplier? Supplier { get; private set; }

	public string? StorageCode { get; private set; }

	public DeliveryDays? Days { get; private set; }

	public TimeOnly? ArrivalTime { get; private set; }

	public DeliveryScheduleBuilder WithSupplier(Supplier supplier)
	{
		Supplier = supplier;
		return this;
	}

	public DeliveryScheduleBuilder WithStorageCode(string storageCode)
	{
		StorageCode = storageCode;
		return this;
	}

	public DeliveryScheduleBuilder WithDays(DeliveryDays days)
	{
		Days = days;
		return this;
	}

	public DeliveryScheduleBuilder WithArrivalTime(TimeOnly arrivalTime)
	{
		ArrivalTime = arrivalTime;
		return this;
	}

	public DeliveryScheduleBuilder WithException(DateOnly date)
	{
		_exceptions.Add(date);
		return this;
	}

	public override DeliverySchedule Build()
	{
		var schedule = DeliverySchedule.Create(
			Supplier ?? Faker.PickRandom<Supplier>(),
			StorageCode ?? Faker.Random.AlphaNumeric(8),
			Days ?? DeliveryDays.Monday, ArrivalTime ?? new TimeOnly(14, 0));

		foreach (var date in _exceptions)
			schedule.AddException(date, DateOnly.MinValue);

		return schedule;
	}
}
