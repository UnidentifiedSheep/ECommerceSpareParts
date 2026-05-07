using FluentValidation;
using Localization.Domain.Extensions;
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
            .WithLocalizationKey("storage.content.restore.list.not.empty");
    }
}