using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageOwners.AddStorageToUser;

public class AddStorageToUserDbValidation : AbstractDbValidation<AddStorageToUserCommand>
{
	public override void Build(IValidationPlan plan, AddStorageToUserCommand request)
	{
		plan
			.ValidateStorageOwnerNotExistsPK((request.StorageCode, request.UserId))
			.ValidateUserExistsId(request.UserId)
			.ValidateStorageExistsCode(request.StorageCode);
	}
}
