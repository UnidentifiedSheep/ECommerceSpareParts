using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Articles.CreateArticles;

public class CreateArticlesDbValidation : AbstractDbValidation<CreateArticlesCommand>
{
    public override void Build(IValidationPlan plan, CreateArticlesCommand request)
    {
        plan.ValidateProducerExistsId(request.NewArticles.Select(x => x.ProducerId));
    }
}