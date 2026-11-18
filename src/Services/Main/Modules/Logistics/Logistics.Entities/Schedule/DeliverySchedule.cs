using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Enums;

namespace Logistics.Entities.Schedule;

public class DeliverySchedule : AuditableEntity<DeliverySchedule, Guid>, ILinqEntity<DeliverySchedule, Guid>
{
	public Guid Id { get; private set; }
	public Supplier Supplier { get; private set; }

	private readonly List<DayOfWeek> _days = [];
	public IReadOnlyList<DayOfWeek> Days => _days;

	public DeliverySchedule() { }

	public DeliverySchedule(Supplier supplier, IEnumerable<DayOfWeek> days)
	{
		Id = Guid.NewGuid();
		Supplier = supplier;
		SetDays(days);
	}

	public void SetDays(IEnumerable<DayOfWeek> days)
	{
		_days.Clear();
		_days.AddRange(days.Distinct());
	}

	public DeliveryScheduleException CreateException(DateTime exceptionDate) => new(Id, exceptionDate);

	public override Guid GetId() => Id;
	public static Expression<Func<DeliverySchedule, Guid>> GetKeySelector() => x => x.Id;
	public static Expression<Func<DeliverySchedule, bool>> GetEqualityExpression(Guid key)
		=> x => x.Id == key;
}
