using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductContent.AddProductContent;

public class AddProductContentDbValidation : AbstractDbValidation<AddProductContentCommand>
{
    public override void Build(IValidationPlan plan, AddProductContentCommand request)
    {
        var ids = request.Contents.Select(x => x.Key).ToHashSet();
        ids.Add(request.ParentProductId);
        plan.ValidateProductExistsId(ids);
    }
}