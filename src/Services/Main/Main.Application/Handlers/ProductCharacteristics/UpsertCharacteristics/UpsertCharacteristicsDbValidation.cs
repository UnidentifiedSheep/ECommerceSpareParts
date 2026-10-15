using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductCharacteristics.UpsertCharacteristics;

public class UpsertCharacteristicsDbValidation : AbstractDbValidation<UpsertCharacteristicsCommand>
{
	public override void Build(IValidationPlan plan, UpsertCharacteristicsCommand request) =>
		plan.ValidateProductExistsId(request.Characteristics.Select(x => x.ProductId))
			.ValidateCharacteristicExistsId(request.Characteristics.Select(x => x.CharacteristicId));
}
