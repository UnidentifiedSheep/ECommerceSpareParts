using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Purchases.GetPurchase;

public class GetPurchasesValidation : AbstractValidator<GetPurchasesQuery>
{
    public GetPurchasesValidation()
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}