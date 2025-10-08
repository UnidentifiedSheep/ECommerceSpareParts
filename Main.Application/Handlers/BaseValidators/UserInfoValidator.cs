using Core.Dtos.Users;
using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class UserInfoValidator : AbstractValidator<UserInfoDto>
{
    public UserInfoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Пользователь должен иметь имя")
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина имени 3 символа")
            .Must(x => !x.Any(char.IsSymbol))
            .WithMessage("Имя не должно содержать спец символов")
            .Must(x => x.Trim().Length <= 30)
            .WithMessage("Максимальная длина имени 30 символов");
        
        RuleFor(x => x.Surname)
            .NotEmpty()
            .WithMessage("Пользователь должен иметь Фамилию")
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина фамилии 3 символа")
            .Must(x => !x.Any(char.IsSymbol))
            .WithMessage("Фамилия не должна содержать спец символов")
            .Must(x => x.Trim().Length <= 30)
            .WithMessage("Максимальная длина фамилии 30 символов");
        
        RuleFor(x => x.Description)
            .Must(x =>  x == null || x.Trim().Length <= 300)
            .WithMessage("Максимальная длина описания 300 символов");
    }
}