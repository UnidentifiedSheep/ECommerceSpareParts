using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Logistics;
using Main.Modules.Storage.Abstractions.Interfaces;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;

public class CreateDeliveryScheduleDbValidation(IStorageChecks storageChecks)
	: AbstractDbValidation<CreateDeliveryScheduleCommand>
{
	public override void Build(IValidationPlan plan, CreateDeliveryScheduleCommand request)
	{
		var storageCode = request.StorageCode.Trim();
		storageChecks.Exists(plan, storageCode);
		plan.ValidateDeliveryScheduleNotExistsPK((request.Supplier, storageCode));
	}
}
