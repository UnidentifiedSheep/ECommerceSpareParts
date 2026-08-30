using Application.Common.Validators;
using Enums;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.GetTransactions;

public class GetTransactionsValidation : AbstractValidator<GetTransactionsQuery>
{
	public GetTransactionsValidation()
	{
		RuleFor(x => new
			{
				x.ReceiverId,
				x.SenderId,
				x.LogicalOperation
			})
			.Must(x => x.ReceiverId != x.SenderId || x.ReceiverId == null)
			.When(x => x.LogicalOperation == LogicalOperation.And)
			.WithLocalizationKey("transaction.sender.receiver.must.not.be.same");

		RuleFor(x => new
			{
				x.ReceiverId, x.SenderId
			})
			.Must(x => x.ReceiverId != null || x.SenderId != null)
			.WithLocalizationKey("transaction.sender.or.receiver.required");

		RuleFor(x => x.DateRange)
			.Must(x => !x.Min.HasValue || !x.Max.HasValue || x.Min.Value.Date <= x.Max.Value.Date)
			.WithLocalizationKey("transaction.range.start.before.end");

		RuleFor(x => x.Cursor).SetValidator(new CursorValidator<(Guid id, DateTime dt)>());
	}
}
