using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentValidation : AbstractValidator<EditStorageContentCommand>
{
    public EditStorageContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.EditedFields).NotEmpty()
            .WithMessage("Список отредактированных элементов не может быть пустым.");

        RuleFor(x => x.EditedFields)
            .Must(x => x.Count < 100)
            .WithMessage("Максимальное количество для редактирования за раз, не может превышать 100 элементов");

        RuleForEach(x => x.EditedFields.Values)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Model.BuyPrice.Value)
                    .SetValidator(new PriceValidator())
                    .When(x => x.Model.BuyPrice.IsSet);

                z.RuleFor(x => x.Model.Count.Value)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Количество у позиции должно быть больше или равно 0")
                    .When(x => x.Model.Count.IsSet);

                z.RuleFor(x => x.Model.PurchaseDatetime.Value.ToUniversalTime())
                    .InclusiveBetween(DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMinutes(10))
                    .When(x => x.Model.PurchaseDatetime.IsSet)
                    .WithMessage("Дата покупки не может быть в будущем, или более чем на 3 месяца в прошлом");
                
                z.RuleFor(x => x.Model.CurrencyId.Value)
                    .CurrencyMustExist(currencyConverter)
                    .When(x => x.Model.CurrencyId.IsSet);
            });
    }
}