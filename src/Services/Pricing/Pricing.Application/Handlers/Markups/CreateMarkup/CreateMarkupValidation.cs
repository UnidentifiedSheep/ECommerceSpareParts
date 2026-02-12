using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Pricing.Application.Handlers.BaseValidators;

namespace Pricing.Application.Handlers.Markups.CreateMarkup;

public class CreateMarkupValidation : AbstractValidator<CreateMarkupCommand>
{
    public CreateMarkupValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.Ranges)
            .NotEmpty()
            .WithMessage("Диапазоны не могут быть пустыми");

        RuleFor(x => x.MarkupRateForUnknownRange)
            .GreaterThan(0)
            .WithMessage("Наценка для неизвестного диапазона не может быть отрицательной или нулевой");

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);


        RuleForEach(x => x.Ranges).ChildRules(context =>
        {
            context.RuleFor(x => x.MarkupRate)
                .SetValidator(new MarkupValidator());
            context.RuleFor(x => x.RangeStart)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Начало диапазона не может быть отрицательным");
            context.RuleFor(x => x.RangeStart)
                .LessThan(x => x.RangeEnd)
                .WithMessage("Начало диапазона должно быть меньше чем конец диапазона");
        });
    }
}