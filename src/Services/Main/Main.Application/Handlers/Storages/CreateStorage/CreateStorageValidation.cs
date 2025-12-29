using FluentValidation;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
    public CreateStorageValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Название не может быть пустым")
            .Must(x => x.Trim().Length >= 6)
            .WithMessage("Минимальная длина названия 6 символов")
            .Must(x => x.Trim().Length <=128)
            .WithMessage("Максимальная длина названия 128 символов");
        RuleFor(x => x.Description)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithMessage("Максимальная длина описания 256 символов");
        RuleFor(x => x.Location)
            .Must(x => x == null || x.Trim().Length <= 256)
            .WithMessage("Максимальная длина локации 256 символов");
    }
}