using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Storages.DeleteStorage;

public class DeleteStorageValidation : AbstractValidator<DeleteStorageCommand>
{
	public DeleteStorageValidation()
	{
		RuleFor(x => x.StorageCode).NotEmpty().WithLocalizableError(StorageCodeNotEmptyMessage.Instance);
	}
}
