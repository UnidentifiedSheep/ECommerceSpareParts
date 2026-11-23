using Exceptions.Base.Localized;
using Main.Entities.Logistics.Schedule;

namespace Main.Entities.Logistics.Exceptions;

public class DeliveryScheduleAlreadyExistsException(DeliveryScheduleKey key)
	: LocalizedConflictException(DeliveryScheduleAlreadyExistsMessage.Instance, key);
