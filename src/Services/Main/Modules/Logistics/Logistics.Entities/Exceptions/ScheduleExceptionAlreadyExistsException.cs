using Enums;
using Exceptions.Base.Localized;

namespace Main.Entities.Logistics.Exceptions;

public class ScheduleExceptionAlreadyExistsException(Supplier supplier, string storageCode, DateOnly date)
	: LocalizedBadRequestException(
		DeliveryScheduleExceptionAlreadyExistsMessage.Instance,
		new
		{
			Supplier = supplier,
			StorageCode = storageCode,
			Date = date
		});
