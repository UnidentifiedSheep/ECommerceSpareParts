using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Markups.GetMarkupGanges;

public class GetMarkupRangesDbValidation : AbstractDbValidation<GetMarkupRangesQuery>
{
    public override void Build(IValidationPlan plan, GetMarkupRangesQuery request)
    {
        plan.ValidateMarkupGroupExistsId(request.GroupId);
    }
}