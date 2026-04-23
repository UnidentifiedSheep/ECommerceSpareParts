using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Products.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenProductsDbValidation : AbstractDbValidation<MakeLinkageBetweenProductsCommand>
{
    public override void Build(IValidationPlan plan, MakeLinkageBetweenProductsCommand request)
    {
        plan.ValidateProductExistsId(request.Linkages
            .SelectMany(x => new[] { x.ProductId, x.CrossProductId }));
    }
}