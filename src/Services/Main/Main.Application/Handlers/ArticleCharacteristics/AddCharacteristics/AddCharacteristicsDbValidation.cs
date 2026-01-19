using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleCharacteristics.AddCharacteristics;

public class AddCharacteristicsDbValidation : AbstractDbValidation<AddCharacteristicsCommand>
{
    public override void Build(IValidationPlan plan, AddCharacteristicsCommand request)
    {
        plan.ValidateArticleExistsId(request.Characteristics.Select(x => x.ArticleId));
    }
}