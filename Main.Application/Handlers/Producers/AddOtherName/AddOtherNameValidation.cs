using FluentValidation;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddOtherNameValidation : AbstractValidator<AddOtherNameCommand>
{
    public AddOtherNameValidation()
    {
        RuleFor(x => x.OtherName)
            .NotEmpty()
            .WithMessage("Дополнительное имя не может быть пустым")
            .Must(name => name.Trim().Length >= 2)
            .WithMessage("Длина дополнительного имени должна быть не менее 2 символов")
            .Must(name => name.Trim().Length <= 64)
            .WithMessage("Длина дополнительного имени не может превышать 64 символов");

        RuleFor(x => x.WhereUsed)
            .Must(name => string.IsNullOrWhiteSpace(name) || name.Trim().Length <= 64)
            .WithMessage("Длина обозначения применения имени не может превышать 64 символов");
    }
}