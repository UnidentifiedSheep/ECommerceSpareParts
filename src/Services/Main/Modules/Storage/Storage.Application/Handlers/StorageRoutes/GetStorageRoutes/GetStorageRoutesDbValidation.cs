using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public class GetStorageRoutesDbValidation : AbstractDbValidation<GetStorageRoutesQuery>
{
	public override void Build(IValidationPlan plan, GetStorageRoutesQuery request)
	{
		if (!string.IsNullOrWhiteSpace(request.StorageFrom))
			plan.ValidateStorageExistsCode(request.StorageFrom);
		if (!string.IsNullOrWhiteSpace(request.StorageTo))
			plan.ValidateStorageExistsCode(request.StorageTo);
	}
}
