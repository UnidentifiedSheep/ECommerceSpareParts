using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Storages.EditStorage;

public class EditStorageValidation : AbstractValidator<EditStorageCommand>
{
	public EditStorageValidation()
	{
		RuleFor(x => x.EditStorage.Description.Value)
			.Must(x => x?.Trim().Length <= 256)
			.When(x => x.EditStorage.Description.IsSet)
			.WithLocalizableError(StorageDescriptionMaxLengthMessage.Instance);

		RuleFor(x => x.EditStorage.Location.Value)
			.Must(x => x?.Trim().Length <= 256)
			.When(x => x.EditStorage.Location.IsSet)
			.WithLocalizableError(StorageLocationMaxLengthMessage.Instance);
	}
}
