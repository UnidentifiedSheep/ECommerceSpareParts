using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Products.Commands.SetProductSizes;

public class SetProductSizesDbValidation : AbstractDbValidation<SetProductSizesCommand>
{
	public override void Build(IValidationPlan plan, SetProductSizesCommand request) =>
		plan.ValidateProductExistsId(request.ProductId);
}
