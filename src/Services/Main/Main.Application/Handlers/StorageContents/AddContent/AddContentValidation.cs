using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.AddContent;

public class AddContentValidation : AbstractValidator<AddContentCommand>
{
    public AddContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleForEach(x => x.StorageContent).ChildRules(content =>
        {
            content.RuleFor(x => x.BuyPrice)
                .SetValidator(new PriceValidator());
            content.RuleFor(x => x.Count)
                .SetValidator(new CountValidator());
            content.RuleFor(x => x.PurchaseDate.ToUniversalTime())
                .InclusiveBetween(DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMinutes(10))
                .WithMessage("Дата покупки не может быть в будущем, или более чем на 3 месяца в прошлом");
            content.RuleFor(x => x.CurrencyId)
                .CurrencyMustExist(currencyConverter);
        });
        RuleFor(x => x.StorageContent)
            .NotEmpty()
            .WithMessage("Список новых позиций не может быть пуст");
        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithMessage("Название склада не может быть пустым");
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не может быть пустым");
        
    }
}