using FluentValidation;

namespace Main.Application.Handlers.Storages.DeleteStorage;

public class DeleteStorageValidation : AbstractValidator<DeleteStorageCommand>
{
    public DeleteStorageValidation()
    {
        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithMessage("Название склада не должно быть пустым");
    }
}