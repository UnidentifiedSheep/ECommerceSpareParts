using FluentValidation;

namespace Application.Handlers.Currencies.CreateCurrency;

public class CreateCurrencyValidation : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidation()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Код валюты не может быть пустым")
            .MaximumLength(26)
            .WithMessage("Максимальная длина кода валюты 26 символов")
            .Must(x => x.Trim().Length >= 2)
            .WithMessage("Минимальная длина кода 2 символа");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Название валюты не может быть пустым")
            .MaximumLength(128)
            .WithMessage("Максимальная длина названия 128 символов")
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина названия 3 символа");
        
        RuleFor(x => x.CurrencySign)
            .NotEmpty()
            .WithMessage("Знак валюты не может быть пустым")
            .MaximumLength(3)
            .WithMessage("Максимальная длина знака валюты 3 символа")
            .Must(x => x.Trim().Length >= 1)
            .WithMessage("Минимальная длина знака валюты 1 символ");
        
        RuleFor(x => x.ShortName)
            .NotEmpty()
            .WithMessage("Короткое название валюты не может быть пустым")
            .MaximumLength(5)
            .WithMessage("Максимальная длина короткого названия 5 символов")
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина короткого названия 2 символа");
    }
}