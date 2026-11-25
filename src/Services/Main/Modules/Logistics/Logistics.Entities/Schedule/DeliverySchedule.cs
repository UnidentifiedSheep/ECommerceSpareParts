using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Enums;
using Main.Entities.Logistics.DomainEvents;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Exceptions;

namespace Main.Entities.Logistics.Schedule;

public sealed class DeliverySchedule : AuditableEntity<DeliverySchedule, DeliveryScheduleKey>,
	ILinqEntity<DeliverySchedule, DeliveryScheduleKey>
{
	public const int StorageCodeMaxLength = 128;

	private readonly List<DeliveryScheduleException> _exceptions = [];

	private DeliverySchedule() { }

	private DeliverySchedule(Supplier supplier, string storageCode, DeliveryDays days, TimeOnly arrivalTime)
	{
		Supplier = supplier;
		StorageCode = storageCode
			.Trim()
			.EnsureNotNullOrEmpty(DeliveryScheduleStorageCodeNotEmptyMessage.Instance)
			.EnsureMaxLength(StorageCodeMaxLength, DeliveryScheduleStorageCodeMaxLengthMessage.Instance);
		SetDays(days);
		SetArrivalTime(arrivalTime);
	}

	[ValidateTuple("PK")]
	public Supplier Supplier { get; }

	[ValidateTuple("PK")]
	public string StorageCode { get; } = null!;

	public DeliveryDays Days { get; private set; }

	public TimeOnly ArrivalTime { get; private set; }

	public IReadOnlyList<DeliveryScheduleException> Exceptions => _exceptions;

	public static Expression<Func<DeliverySchedule, DeliveryScheduleKey>> GetKeySelector()
		=> x => new DeliveryScheduleKey(x.Supplier, x.StorageCode);

	public static Expression<Func<DeliverySchedule, bool>> GetEqualityExpression(DeliveryScheduleKey key)
		=> x => x.Supplier == key.Supplier && x.StorageCode == key.StorageCode;

	public static DeliverySchedule Create(
		Supplier supplier,
		string storageCode,
		DeliveryDays days,
		TimeOnly arrivalTime)
		=> new(supplier, storageCode, days, arrivalTime);

	public void SetDays(DeliveryDays days)
	{
		Days = days
			.EnsureNotDefault(DeliveryScheduleDaysNotEmptyMessage.Instance)
			.EnsureOnlyDefinedFlags(DeliveryScheduleDaysUnsupportedMessage.Instance);
		RaiseChanged();
	}

	public void SetArrivalTime(TimeOnly arrivalTime)
	{
		ArrivalTime = arrivalTime;
		RaiseChanged();
	}

	public DeliveryScheduleException AddException(DateOnly date, DateOnly today)
	{
		date.EnsureAtLeast(today, DeliveryScheduleExceptionDateInPastMessage.Instance);
		if (_exceptions.Any(exception => exception.Date == date))
			throw new ScheduleExceptionAlreadyExistsException(Supplier, StorageCode, date);

		var exception = DeliveryScheduleException.Create(Supplier, StorageCode, date);
		_exceptions.Add(exception);
		RaiseChanged();
		return exception;
	}

	public void RemoveException(DateOnly date)
	{
		var exception = _exceptions.FirstOrDefault(item => item.Date == date) ??
			throw new ScheduleExceptionNotFoundException(Supplier, StorageCode, date);
		_exceptions.Remove(exception);
		RaiseChanged();
	}

	public override void OnCreated()
	{
		base.OnCreated();
		RaiseChanged();
	}

	public override void OnDeleted()
	{
		base.OnDeleted();
		RaiseChanged();
	}

	public override DeliveryScheduleKey GetId() => new(Supplier, StorageCode);

	private void RaiseChanged()
		=> AddDomainEvent(new DeliveryScheduleChangedDomainEvent(Supplier, StorageCode));
}

public readonly record struct DeliveryScheduleKey(Supplier Supplier, string StorageCode) : ICompositeKey
{
	public object[] ToArray() => [Supplier, StorageCode];
}
