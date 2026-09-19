using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
	public CreateStorageValidation()
	{
		RuleFor(x => x.Code)
			.NotEmpty()
			.WithLocalizableError(StorageCodeNotEmptyMessage.Instance)
			.Must(x => x.Trim().Length >= 6)
			.WithLocalizableError(StorageCodeMinLengthMessage.Instance)
			.Must(x => x.Trim().Length <= 128)
			.WithLocalizableError(StorageCodeMaxLengthMessage.Instance);

		RuleFor(x => x.Description)
			.Must(x => x == null || x.Trim().Length <= 256)
			.WithLocalizableError(StorageDescriptionMaxLengthMessage.Instance);

		RuleFor(x => x.Location)
			.Must(x => x == null || x.Trim().Length <= 256)
			.WithLocalizableError(StorageLocationMaxLengthMessage.Instance);
	}
}
