using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Products.SetProductSizes;

public class SetProductSizesDbValidation : AbstractDbValidation<SetProductSizesCommand>
{
	public override void Build(IValidationPlan plan, SetProductSizesCommand request)
		=> plan.ValidateProductExistsId(request.ProductId);
}
