using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Currency;
using Main.Entities.Storage;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

public class EditStorageRouteDbValidation : AbstractDbValidation<EditStorageRouteCommand>
{
	public override void Build(IValidationPlan plan, EditStorageRouteCommand request)
	{
		if (request.PatchStorageRoute.CarrierId is { IsSet: true, Value: not null })
			plan.ValidateUserExistsId(request.PatchStorageRoute.CarrierId.Value.Value);

		if (request.PatchStorageRoute.CurrencyId.IsSet)
			plan.ValidateCurrencyExistsId(request.PatchStorageRoute.CurrencyId.Value);
	}
}
