using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Products.Commands.MakeLinkageBetweenArticles;

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
