using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Sales.GetSales;

public class GetSalesValidation : AbstractValidator<GetSalesQuery>
{
    public GetSalesValidation()
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithLocalizationKey("sale.range.start.before.end");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithLocalizationKey("sale.range.max5months");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}