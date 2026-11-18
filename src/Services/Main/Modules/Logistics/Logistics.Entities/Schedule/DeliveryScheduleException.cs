using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Logistics.Entities.Schedule;

public class DeliveryScheduleException : Entity<DeliveryScheduleException, ScheduleExceptionKey>,
	ILinqEntity<DeliveryScheduleException, ScheduleExceptionKey>
{
	public Guid DeliveryScheduleId { get; private set; }
	public DateTime Date { get; private set; }

	private DeliveryScheduleException() { }

	internal DeliveryScheduleException(Guid deliveryScheduleId, DateTime exceptionDate)
	{
		DeliveryScheduleId = deliveryScheduleId;
		Date = exceptionDate.Date;

		if (DateTime.Today > Date)
			throw new InvalidOperationException("The date cannot be in the past.");
	}

	public override ScheduleExceptionKey GetId() => new(DeliveryScheduleId, Date);
	public static Expression<Func<DeliveryScheduleException, ScheduleExceptionKey>> GetKeySelector()
		=> x => new(x.DeliveryScheduleId, x.Date);
	public static Expression<Func<DeliveryScheduleException, bool>> GetEqualityExpression(ScheduleExceptionKey key)
		=> x => x.DeliveryScheduleId == key.DeliveryScheduleId && x.Date == key.Date;
}

public readonly record struct ScheduleExceptionKey(Guid DeliveryScheduleId, DateTime Date) : ICompositeKey
{
	public object[] ToArray() => [DeliveryScheduleId, Date];
}
