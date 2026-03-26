using FluentValidation;
using Localization.Domain.Extensions;
using Main.Abstractions.Dtos.Amw.Sales;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class EditSaleContentsValidator : AbstractValidator<IEnumerable<EditSaleContentDto>>
{
    public EditSaleContentsValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithLocalizationKey("sale.content.list.not.empty");

        RuleFor(x => x)
            .Must(list =>
            {
                var seen = new HashSet<int>();
                foreach (var item in list)
                    if (item.Id.HasValue && !seen.Add(item.Id.Value))
                        return false;
                return true;
            })
            .WithLocalizationKey("sale.content.list.duplicate");

        RuleForEach(x => x)
            .SetValidator(new EditSaleContentValidator());
    }
}