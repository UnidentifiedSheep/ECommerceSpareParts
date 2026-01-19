using FluentValidation;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Purchases.BaseValidators.Edit;

public class EditPurchaseDtoValidation : AbstractValidator<IEnumerable<EditPurchaseDto>>
{
    public EditPurchaseDtoValidation()
    {
        RuleForEach(z => z)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Count)
                    .SetValidator(new CountValidator());

                z.RuleFor(x => x.Price)
                    .SetValidator(new PriceValidator());
            });

        RuleFor(z => z)
            .Must(z =>
            {
                var ids = z.Where(x => x.Id != null).Select(x => x.Id!.Value).ToList();
                var idsSet = ids.ToHashSet();
                return ids.Count == idsSet.Count;
            })
            .WithMessage("Дубликаты номеров позиций не разрешены.");
    }
}