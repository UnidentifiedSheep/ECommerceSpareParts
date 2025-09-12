using FluentValidation;

namespace Application.Handlers.Storages.EditStorage;

public class EditStorageValidation : AbstractValidator<EditStorageCommand>
{
    public EditStorageValidation()
    {
        RuleFor(x => x.EditStorage.Description.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Description.IsSet)
            .WithMessage("Максимальная длина описания 256 символов");

        RuleFor(x => x.EditStorage.Location.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Location.IsSet)
            .WithMessage("Максимальная длина локации 256 символов");
    }
}