using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Products.Commands.UpsertCharacteristics;

public class UpsertCharacteristicsDbValidation : AbstractDbValidation<UpsertCharacteristicsCommand>
{
	public override void Build(IValidationPlan plan, UpsertCharacteristicsCommand request)
	{
		if (request.Characteristics.Count == 0)
			return;

		plan.ValidateProductExistsId(request.Characteristics.Select(x => x.ProductId))
			.ValidateCharacteristicExistsId(request.Characteristics.Select(x => x.CharacteristicId));
	}
}
