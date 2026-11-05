using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Products.Commands.MapImgsToProduct;

public class MapImgsToProductDbValidation : AbstractDbValidation<MapImgsToProductCommand>
{
	public override void Build(IValidationPlan plan, MapImgsToProductCommand request) =>
		plan.ValidateProductExistsId(request.ProductId);
}
