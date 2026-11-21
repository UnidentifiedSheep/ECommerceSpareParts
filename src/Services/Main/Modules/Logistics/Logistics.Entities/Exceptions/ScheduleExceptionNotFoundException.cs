using Enums;
using Exceptions.Base.Localized;

namespace Main.Entities.Logistics.Exceptions;

public class ScheduleExceptionNotFoundException(Supplier supplier, string storageCode, DateOnly date)
	: LocalizedNotFoundException(
		DeliveryScheduleExceptionNotFoundMessage.Instance,
		new
		{
			Supplier = supplier,
			StorageCode = storageCode,
			Date = date
		});
