using Core.Extensions;
using Core.Interfaces;
using FluentValidation;

namespace Application.Handlers.Users.CreateUser;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation(IEmailValidator emailValidator)
    {
        //Name Rules
        RuleFor(x => x.NewUser.Name)
            .NotEmpty()
            .WithMessage("Имя пользователя не должно быть пустым");
        RuleFor(x => x.NewUser)
            .Must(x => x.Name.Trim().Length >= 3)
            .WithMessage("Имя пользователя должно состоять минимум из 3 символов");
        //Username Rules
        RuleFor(x => x.NewUser.UserName)
            .NotEmpty()
            .WithMessage("Логин пользователя не может быть пустым");
        RuleFor(x => x.NewUser.UserName.Length)
            .GreaterThanOrEqualTo(3)
            .WithMessage("Минимальная длина логина пользователя 3 символа");
        
        //Email Rules 
        RuleFor(x => x.NewUser)
            .Must(x => emailValidator.IsValidEmail(x.Email!))
            .When(x => !string.IsNullOrEmpty(x.NewUser.Email))
            .WithMessage(x => $"'{x.NewUser.Email}' не является валидной почтой");
        //Phone Number
        RuleFor(x => x.NewUser.PhoneNumber).Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrWhiteSpace(x.NewUser.PhoneNumber))
            .WithMessage(x => $"'{x.NewUser.PhoneNumber}' не является валидным номером телефона");
    }
}