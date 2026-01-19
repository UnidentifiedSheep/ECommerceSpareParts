using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public class RestoreContentValidation : AbstractValidator<RestoreContentCommand>
{
    public RestoreContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleForEach(z => z.ContentDetails)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Detail.Count)
                    .SetValidator(new CountValidator());
                z.RuleFor(x => x.Detail.BuyPrice)
                    .SetValidator(new PriceValidator());

                z.RuleFor(x => x.Detail.CurrencyId)
                    .CurrencyMustExist(currencyConverter);
            });

        RuleFor(x => x.ContentDetails)
            .NotEmpty()
            .WithMessage("Список для восстановления не должен быть пуст");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не должен быть пуст");
    }
}