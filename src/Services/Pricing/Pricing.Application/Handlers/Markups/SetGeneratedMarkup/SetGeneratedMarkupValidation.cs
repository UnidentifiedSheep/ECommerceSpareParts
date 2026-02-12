using FluentValidation;

namespace Pricing.Application.Handlers.Markups.SetGeneratedMarkup;

public class SetGeneratedMarkupValidation : AbstractValidator<SetGeneratedMarkupCommand>
{
    public SetGeneratedMarkupValidation()
    {
        RuleFor(x => x.Ranges)
            .NotEmpty()
            .WithMessage("Диапазоны наценок не могут быть пусты");
    }
}