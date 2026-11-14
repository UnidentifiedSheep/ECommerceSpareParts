using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Storages.DeleteStorage;

public class DeleteStorageValidation : AbstractValidator<DeleteStorageCommand>
{
	public DeleteStorageValidation()
		=> RuleFor(x => x.StorageCode).NotEmpty().WithLocalizableError(StorageCodeNotEmptyMessage.Instance);
}
