using FluentValidation;

namespace Main.Application.Handlers.Permissions.CreatePermission;

public class CreatePermissionValidation : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionValidation()
    {
        RuleFor(x => x.Name)
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина 'разрешения' 3 символа")
            .Must(x => x.Trim().Length <= 128)
            .WithMessage("Максимальная длина 'разрешения' 128 символа");

        RuleFor(x => x.Description)
            .Must(x => x?.Trim().Length <= 256)
            .WithMessage("Максимальная длина описания 256 символа");
    }
}