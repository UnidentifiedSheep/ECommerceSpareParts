using FluentValidation;

namespace Application.Handlers.Storages.CreateStorage;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
    public CreateStorageValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Название не может быть пустым")
            .MinimumLength(6)
            .WithMessage("Минимальная длина названия 6 символов")
            .MaximumLength(128)
            .WithMessage("Максимальная длина названия 128 символов");
        RuleFor(x => x.Description)
            .MaximumLength(256)
            .WithMessage("Максимальная длина описания 256 символов");
        RuleFor(x => x.Location)
            .MaximumLength(256)
            .WithMessage("Максимальная длина локации 256 символов");
    }
}