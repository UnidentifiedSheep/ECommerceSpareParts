using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.GetSales;

public class GetSalesValidation : AbstractValidator<GetSalesQuery>
{
    public GetSalesValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}