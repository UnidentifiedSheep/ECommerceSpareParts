using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public class GetStorageOwnersDbValidation : AbstractDbValidation<GetStorageOwnersQuery>
{
	public override void Build(IValidationPlan plan, GetStorageOwnersQuery request)
		=> plan.ValidateStorageExistsCode(request.StorageCode);
}
