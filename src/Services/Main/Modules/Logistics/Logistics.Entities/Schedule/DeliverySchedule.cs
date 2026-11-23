using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Enums;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Extensions;

namespace Main.Entities.Logistics.Schedule;

public sealed class DeliverySchedule : AuditableEntity<DeliverySchedule, DeliveryScheduleKey>,
	ILinqEntity<DeliverySchedule, DeliveryScheduleKey>
{
	public const int StorageCodeMaxLength = 128;

	private readonly List<DeliveryScheduleException> _exceptions = [];

	private DeliverySchedule() { }

	private DeliverySchedule(Supplier supplier, string storageCode, DeliveryDays days)
	{
		Supplier = supplier;
		StorageCode = storageCode
			.Trim()
			.EnsureNotNullOrEmpty(DeliveryScheduleStorageCodeNotEmptyMessage.Instance)
			.EnsureMaxLength(StorageCodeMaxLength, DeliveryScheduleStorageCodeMaxLengthMessage.Instance);
		SetDays(days);
	}

	[ValidateTuple("PK")]
	public Supplier Supplier { get; }

	[ValidateTuple("PK")]
	public string StorageCode { get; } = null!;

	public DeliveryDays Days { get; private set; }

	public IReadOnlyList<DeliveryScheduleException> Exceptions => _exceptions;

	public static Expression<Func<DeliverySchedule, DeliveryScheduleKey>> GetKeySelector()
		=> x => new DeliveryScheduleKey(x.Supplier, x.StorageCode);

	public static Expression<Func<DeliverySchedule, bool>> GetEqualityExpression(DeliveryScheduleKey key)
		=> x => x.Supplier == key.Supplier && x.StorageCode == key.StorageCode;

	public static DeliverySchedule Create(Supplier supplier, string storageCode, DeliveryDays days)
		=> new(supplier, storageCode, days);

	public void SetDays(DeliveryDays days)
		=> Days = days
			.EnsureNotDefault(DeliveryScheduleDaysNotEmptyMessage.Instance)
			.EnsureOnlyDefinedFlags(DeliveryScheduleDaysUnsupportedMessage.Instance);

	public DeliveryScheduleException AddException(DateOnly date, DateOnly today)
	{
		date.EnsureAtLeast(today, DeliveryScheduleExceptionDateInPastMessage.Instance);
		if (_exceptions.Any(exception => exception.Date == date))
			throw new ScheduleExceptionAlreadyExistsException(Supplier, StorageCode, date);

		var exception = DeliveryScheduleException.Create(Supplier, StorageCode, date);
		_exceptions.Add(exception);
		return exception;
	}

	public void RemoveException(DateOnly date)
	{
		var exception = _exceptions.FirstOrDefault(item => item.Date == date) ??
			throw new ScheduleExceptionNotFoundException(Supplier, StorageCode, date);
		_exceptions.Remove(exception);
	}

	public bool IsDeliveryDay(DateOnly date)
		=> Days.Contains(date.DayOfWeek) && _exceptions.All(exception => exception.Date != date);

	public DateOnly GetNextDeliveryDate(DateOnly from)
	{
		var exceptions = _exceptions.Select(exception => exception.Date).ToHashSet();
		var date = from;
		while (true)
		{
			var offset = Days.DaysUntilNext(date.DayOfWeek) ??
				throw new InvalidOperationException(
					$"Delivery schedule {Supplier}/{StorageCode} has no delivery days.");
			date = date.AddDays(offset);
			if (!exceptions.Contains(date))
				return date;

			date = date.AddDays(1);
		}
	}

	public override DeliveryScheduleKey GetId() => new(Supplier, StorageCode);
}

public readonly record struct DeliveryScheduleKey(Supplier Supplier, string StorageCode) : ICompositeKey
{
	public object[] ToArray() => [Supplier, StorageCode];
}
