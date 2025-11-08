using FluentValidation;

namespace Main.Application.Handlers.StorageContents.MoveContentToOtherStorage;

public class MoveContentToOtherStorageValidation : AbstractValidator<MoveContentToOtherStorageCommand>
{
    public MoveContentToOtherStorageValidation()
    {
        RuleFor(x => x.Movements)
            .Must(x =>
            {
                var all = x.ToList();
                var ids = all.Select(z => z.StorageContentId).ToHashSet();
                return all.Count == ids.Count;
            })
            .WithMessage("Список не должен содержать дубликатов позиций")
            .NotEmpty()
            .WithMessage("Список не должен быть пустым");
    }
}