using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.Handlers.StorageOwners.AddStorageToUser;

public class AddStorageToUserDbValidation : AbstractDbValidation<AddStorageToUserCommand>
{
	public override void Build(IValidationPlan plan, AddStorageToUserCommand request)
		=> plan
			.ValidateStorageOwnerNotExistsPK((request.StorageCode, request.UserId))
			.ValidateUserExistsId(request.UserId)
			.ValidateStorageExistsCode(request.StorageCode);
}
