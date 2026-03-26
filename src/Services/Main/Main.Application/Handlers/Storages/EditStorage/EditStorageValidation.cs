using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Storages.EditStorage;

public class EditStorageValidation : AbstractValidator<EditStorageCommand>
{
    public EditStorageValidation()
    {
        RuleFor(x => x.EditStorage.Description.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Description.IsSet)
            .WithLocalizationKey("storage.description.max.length");

        RuleFor(x => x.EditStorage.Location.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Location.IsSet)
            .WithLocalizationKey("storage.location.max.length");
    }
}