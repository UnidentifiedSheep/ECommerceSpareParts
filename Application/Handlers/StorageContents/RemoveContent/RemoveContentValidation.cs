using FluentValidation;

namespace Application.Handlers.StorageContents.RemoveContent;

public class RemoveContentValidation : AbstractValidator<RemoveContentCommand>
{
    public RemoveContentValidation()
    {
        RuleForEach(x => x.Content)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .GreaterThan(0)
                    .WithMessage("Количество которое надо убрать со склада не может быть 0 или отрицательным число");
            });

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Список на уменьшение количества не должен быть пустым.");

        RuleFor(x => x.StorageName)
            .NotEmpty()
            .When(x => !x.TakeFromOtherStorages)
            .WithMessage(
                "Нельзя менять количество артикула, не выбрав склад и не разрешая менять количество на других складах");
    }
}