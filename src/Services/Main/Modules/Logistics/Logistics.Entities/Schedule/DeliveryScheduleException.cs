using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Logistics.Entities.Schedule;

public class DeliveryScheduleException : Entity<DeliveryScheduleException, ScheduleExceptionKey>,
	ILinqEntity<DeliveryScheduleException, ScheduleExceptionKey>
{
	public Guid DeliveryScheduleId { get; private set; }
	public int Year { get; private set; }
	public int Month { get; private set; }
	public int Day { get; private set; }

	private DeliveryScheduleException() { }

	internal DeliveryScheduleException(Guid deliveryScheduleId, DateTime exceptionDate)
	{
		DeliveryScheduleId = deliveryScheduleId;
		Year = exceptionDate.Year;
		Month = exceptionDate.Month;
		Day = exceptionDate.Day;
	}

	public override ScheduleExceptionKey GetId() => new(DeliveryScheduleId, Year, Month, Day);
	public static Expression<Func<DeliveryScheduleException, ScheduleExceptionKey>> GetKeySelector()
		=> x => new(x.DeliveryScheduleId, x.Year, x.Month, x.Day);
	public static Expression<Func<DeliveryScheduleException, bool>> GetEqualityExpression(ScheduleExceptionKey key)
		=> x => x.DeliveryScheduleId == key.DeliveryScheduleId &&
			x.Year == key.Year &&
			x.Month == key.Month &&
			x.Day == key.Day;
}

public readonly record struct ScheduleExceptionKey(
	Guid DeliveryScheduleId,
	int Year,
	int Month,
	int Day) : ICompositeKey
{
	public object[] ToArray() => [DeliveryScheduleId, Year, Month, Day];
}
