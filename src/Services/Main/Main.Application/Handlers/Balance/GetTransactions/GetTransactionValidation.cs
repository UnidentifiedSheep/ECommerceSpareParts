using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.GetTransactions;

public class GetTransactionsValidation : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidation()
    {
        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != x.SenderId || x.ReceiverId == null)
            .WithLocalizationKey("transaction.sender.receiver.must.not.be.same");

        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != null || x.SenderId != null)
            .WithLocalizationKey("transaction.sender.or.receiver.required");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart.Date <= x.RangeEnd.Date)
            .WithLocalizationKey("transaction.range.start.before.end");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeEnd.Date <= x.RangeStart.Date.AddMonths(5))
            .WithLocalizationKey("transaction.range.max.months");

        RuleFor(x => x.Cursor)
            .SetValidator(new CursorValidator<(Guid id, DateTime dt)>());
    }
}