using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public class RestoreContentDbValidation : AbstractDbValidation<RestoreContentCommand>
{
    public override void Build(IValidationPlan plan, RestoreContentCommand request)
    {
        plan.ValidateCurrencyExistsId(request.ContentDetails.Select(x => x.Detail.CurrencyId).Distinct());
    }
}