using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Characteristics.DeleteCharacteristic;

public sealed class DeleteCharacteristicDbValidation : AbstractDbValidation<DeleteCharacteristicCommand>
{
	public override void Build(IValidationPlan plan, DeleteCharacteristicCommand request) =>
		plan.ValidateProductCharacteristicNotExistsCharacteristicId(request.Id);
}
