using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentValidation : AbstractValidator<EditStorageContentCommand>
{
    public EditStorageContentValidation()
    {
        RuleFor(x => x.EditedFields).NotEmpty()
            .WithMessage("Список отредактированных элементов не может быть пустым.");

        RuleFor(x => x.EditedFields)
            .Must(x => x.Count < 100)
            .WithMessage("Максимальное количество для редактирования за раз, не может превышать 100 элементов");

        RuleForEach(x => x.EditedFields.Values)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.value.BuyPrice.Value)
                    .SetValidator(new PriceValidator())
                    .When(x => x.value.BuyPrice.IsSet);

                z.RuleFor(x => x.value.Count.Value)
                    .SetValidator(new CountValidator())
                    .When(x => x.value.Count.IsSet);

                z.RuleFor(x => x.value.PurchaseDatetime.Value)
                    .InclusiveBetween(DateTime.Now.AddMonths(-3), DateTime.Now.AddMinutes(10))
                    .When(x => x.value.PurchaseDatetime.IsSet)
                    .WithMessage("Дата покупки не может быть в будущем, или более чем на 3 месяца в прошлом");
            });
    }
}