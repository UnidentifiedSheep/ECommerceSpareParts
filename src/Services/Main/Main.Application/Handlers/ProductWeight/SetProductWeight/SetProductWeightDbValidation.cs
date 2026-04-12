using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleWeight.SetArticleWeight;

public class SetProductWeightDbValidation : AbstractDbValidation<SetArticleWeightCommand>
{
    public override void Build(IValidationPlan plan, SetArticleWeightCommand request)
    {
        plan.ValidateProductExistsId(request.ProductId);
    }
}