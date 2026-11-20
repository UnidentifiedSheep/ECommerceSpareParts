using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Products.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenProductsDbValidation : AbstractDbValidation<MakeLinkageBetweenProductsCommand>
{
	public override void Build(IValidationPlan plan, MakeLinkageBetweenProductsCommand request)
	{
		if (request.Linkages.Count == 0)
			return;

		plan.ValidateProductExistsId(
			request.Linkages.SelectMany(x => new[]
			{
				x.ProductId, x.CrossProductId
			}));
	}
}
