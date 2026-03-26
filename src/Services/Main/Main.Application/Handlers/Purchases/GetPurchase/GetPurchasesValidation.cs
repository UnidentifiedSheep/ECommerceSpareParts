using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.GetPurchase;

public class GetPurchasesValidation : AbstractValidator<GetPurchasesQuery>
{
    public GetPurchasesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithLocalizationKey("purchase.date.range.start.before.end");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithLocalizationKey("purchase.date.range.max.5months");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}