using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
    public CreateStorageValidation()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithLocalizationKey("storage.code.not.empty")
            .Must(x => x.Trim().Length >= 6)
            .WithLocalizationKey("storage.code.min.length")
            .Must(x => x.Trim().Length <= 128)
            .WithLocalizationKey("storage.code.max.length");

        RuleFor(x => x.Description)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithLocalizationKey("storage.description.max.length");

        RuleFor(x => x.Location)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithLocalizationKey("storage.location.max.length");
    }
}
