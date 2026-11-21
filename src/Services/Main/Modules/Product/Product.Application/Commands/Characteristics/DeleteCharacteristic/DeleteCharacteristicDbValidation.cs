using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Characteristics.DeleteCharacteristic;

public sealed class DeleteCharacteristicDbValidation : AbstractDbValidation<DeleteCharacteristicCommand>
{
	public override void Build(IValidationPlan plan, DeleteCharacteristicCommand request)
		=> plan.ValidateProductCharacteristicNotExistsCharacteristicId(request.Id);
}
