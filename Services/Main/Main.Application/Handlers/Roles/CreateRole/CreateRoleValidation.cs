using FluentValidation;

namespace Main.Application.Handlers.Roles.CreateRole;

public class CreateRoleValidation : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidation()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Название роли не может быть пустым")
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина названия роли 3 символа")
            .Must(x => x.Trim().Length <= 24)
            .WithMessage("Максимальная длина названия роли 24 символа");
    }
}