using FluentValidation;
using Main.Core.Dtos.Amw.Sales;

namespace Main.Application.Handlers.Sales.BaseValidators.Edit;

public class SaleContentValidator : AbstractValidator<IEnumerable<EditSaleContentDto>>
{
    public SaleContentValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Список содержимого продажи не может быть пустым.");

        RuleFor(x => x)
            .Must(list =>
            {
                var seen = new HashSet<int>();
                foreach (var item in list)
                    if (item.Id.HasValue && !seen.Add(item.Id.Value))
                        return false;

                return true;
            })
            .WithMessage("Не может быть две позиции с одинаковым идентификатором");

        RuleForEach(x => x)
            .SetValidator(new EditSaleContentDtoValidator());
    }
}