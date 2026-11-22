using Exceptions.Base.Localized;
using Main.Entities.Logistics.Schedule;

namespace Main.Entities.Logistics.Exceptions;

public class DeliveryScheduleNotFoundException(DeliveryScheduleKey key)
	: LocalizedNotFoundException(DeliveryScheduleNotFoundMessage.Instance, key);
