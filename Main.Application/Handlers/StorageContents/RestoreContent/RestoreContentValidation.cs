using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public class RestoreContentValidation : AbstractValidator<RestoreContentCommand>
{
    public RestoreContentValidation()
    {
        RuleForEach(z => z.ContentDetails)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Detail.Count)
                    .SetValidator(new CountValidator());
                z.RuleFor(x => x.Detail.BuyPrice)
                    .SetValidator(new PriceValidator());
            });

        RuleFor(x => x.ContentDetails)
            .NotEmpty()
            .WithMessage("Список для восстановления не должен быть пуст");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не должен быть пуст");
    }
}