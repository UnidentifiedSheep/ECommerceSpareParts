using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Permissions.CreatePermission;

public class CreatePermissionValidation : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionValidation()
    {
        RuleFor(x => x.Name)
            .Must(x => x.Trim().Length >= 3)
            .WithLocalizationKey("permission.name.min.length")
            .Must(x => x.Trim().Length <= 128)
            .WithLocalizationKey("permission.name.max.length");

        RuleFor(x => x.Description)
            .Must(x => x?.Trim().Length <= 256)
            .WithLocalizationKey("permission.description.max.length");
    }
}