using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Logistics.Schedule;

public sealed class DeliveryScheduleException : Entity<DeliveryScheduleException, ScheduleExceptionKey>,
	ILinqEntity<DeliveryScheduleException, ScheduleExceptionKey>
{
	private DeliveryScheduleException()
	{
	}

	private DeliveryScheduleException(Supplier supplier, string storageCode, DateOnly date)
	{
		Supplier = supplier;
		StorageCode = storageCode;
		Date = date;
	}

	public Supplier Supplier { get; }

	public string StorageCode { get; } = null!;

	public DateOnly Date { get; }

	public static Expression<Func<DeliveryScheduleException, ScheduleExceptionKey>> GetKeySelector()
		=> x => new ScheduleExceptionKey(x.Supplier, x.StorageCode, x.Date);

	public static Expression<Func<DeliveryScheduleException, bool>> GetEqualityExpression(ScheduleExceptionKey key)
		=> x => x.Supplier == key.Supplier && x.StorageCode == key.StorageCode && x.Date == key.Date;

	internal static DeliveryScheduleException Create(Supplier supplier, string storageCode, DateOnly date)
		=> new(supplier, storageCode, date);

	public override ScheduleExceptionKey GetId() => new(Supplier, StorageCode, Date);
}

public readonly record struct ScheduleExceptionKey(Supplier Supplier, string StorageCode, DateOnly Date) : ICompositeKey
{
	public object[] ToArray() => [Supplier, StorageCode, Date];
}
