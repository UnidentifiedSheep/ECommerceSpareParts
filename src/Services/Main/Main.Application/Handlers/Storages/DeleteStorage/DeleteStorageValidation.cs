using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Storages.DeleteStorage;

public class DeleteStorageValidation : AbstractValidator<DeleteStorageCommand>
{
	public DeleteStorageValidation()
	{
		RuleFor(x => x.StorageCode).NotEmpty().WithLocalizationKey("storage.code.not.empty");
	}
}
