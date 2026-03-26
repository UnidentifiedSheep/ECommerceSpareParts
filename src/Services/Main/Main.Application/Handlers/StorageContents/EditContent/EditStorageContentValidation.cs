using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentValidation : AbstractValidator<EditStorageContentCommand>
{
    public EditStorageContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.EditedFields)
            .NotEmpty()
            .WithLocalizationKey("storage.content.edit.list.not.empty");

        RuleFor(x => x.EditedFields)
            .Must(x => x.Count < 100)
            .WithLocalizationKey("storage.content.edit.max.count");

        RuleForEach(x => x.EditedFields.Values)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Model.BuyPrice.Value)
                    .SetValidator(new PriceValidator())
                    .When(x => x.Model.BuyPrice.IsSet);

                z.RuleFor(x => x.Model.Count.Value)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Model.Count.IsSet)
                    .WithLocalizationKey("storage.content.count.min.zero");

                z.RuleFor(x => x.Model.PurchaseDatetime.Value.ToUniversalTime())
                    .InclusiveBetween(DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMinutes(10))
                    .When(x => x.Model.PurchaseDatetime.IsSet)
                    .WithLocalizationKey("storage.content.purchase.date.range");

                z.RuleFor(x => x.Model.CurrencyId.Value)
                    .CurrencyMustExist(currencyConverter)
                    .When(x => x.Model.CurrencyId.IsSet);
            });
    }
}