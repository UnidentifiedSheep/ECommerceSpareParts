using BulkValidation.Core.Configuration;
using Main.Entities.Logistics;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Schedule;

namespace Main.Modules.Logistics.Application.Configs;

public static class LogisticsValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
		=> errors.For(DeliveryScheduleKeys.PK)
			.OnAlreadyExists(input =>
				new DeliveryScheduleAlreadyExistsException(
					new DeliveryScheduleKey(input.Values[0].Supplier, input.Values[0].StorageCode)));
}
