using FluentValidation;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.BaseValidators.Edit;

public class EditSaleContentDtoValidator : AbstractValidator<EditSaleContentDto>
{
    public EditSaleContentDtoValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("Количество должно быть больше 0.");

        RuleFor(x => x.Comment)
            .Must(x => x?.Trim().Length <= 256)
            .WithMessage("Максимальная длина комментария у позиции — 256 символов.");

        RuleFor(x => x.Price)
            .SetValidator(new PriceValidator());

        RuleFor(x => x.PriceWithDiscount)
            .SetValidator(new PriceValidator())
            .LessThanOrEqualTo(x => x.Price)
            .WithMessage("Цена со скидкой не может быть больше оригинальной цены.");
    }
}