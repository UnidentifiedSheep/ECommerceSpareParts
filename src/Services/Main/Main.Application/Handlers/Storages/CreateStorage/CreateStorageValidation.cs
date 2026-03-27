using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
    public CreateStorageValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithLocalizationKey("storage.name.not.empty")
            .Must(x => x.Trim().Length >= 6)
            .WithLocalizationKey("storage.name.min.length")
            .Must(x => x.Trim().Length <= 128)
            .WithLocalizationKey("storage.name.max.length");

        RuleFor(x => x.Description)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithLocalizationKey("storage.description.max.length");

        RuleFor(x => x.Location)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithLocalizationKey("storage.location.max.length");
    }
}