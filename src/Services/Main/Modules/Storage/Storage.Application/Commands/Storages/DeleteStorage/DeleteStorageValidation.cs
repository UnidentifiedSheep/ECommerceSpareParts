using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Storage;

namespace Main.Modules.Storage.Application.Commands.Storages.DeleteStorage;

public class DeleteStorageValidation : AbstractValidator<DeleteStorageCommand>
{
	public DeleteStorageValidation()
		=> RuleFor(x => x.StorageCode).NotEmpty().WithLocalizableError(StorageCodeNotEmptyMessage.Instance);
}
