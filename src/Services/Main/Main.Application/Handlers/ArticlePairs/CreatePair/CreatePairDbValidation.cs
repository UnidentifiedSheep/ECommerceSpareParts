using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticlePairs.CreatePair;

public class CreatePairDbValidation : AbstractDbValidation<CreatePairCommand>
{
    public override void Build(IValidationPlan plan, CreatePairCommand request)
    {
        plan.ValidateArticleExistsId([request.LeftArticleId, request.RightArticleId]);
    }
}