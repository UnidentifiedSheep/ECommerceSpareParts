using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Sales.GetSales;

public class GetSalesValidation : AbstractValidator<GetSalesQuery>
{
    public GetSalesValidation()
    {
        RuleFor(query => query.DateRange)
            .Must(x => !x.Min.HasValue || !x.Max.HasValue || x.Min.Value.Date <= x.Max.Value.Date)
            .WithLocalizationKey("sale.range.start.before.end");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}