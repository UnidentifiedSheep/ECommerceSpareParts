using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

public class ReverseTransactionValidation : AbstractValidator<ReverseTransactionCommand>
{
    public ReverseTransactionValidation()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithLocalizationKey("transaction.id.required");

        RuleFor(x => x.WhoReversed)
            .NotEmpty()
            .WithLocalizationKey("transaction.who.delete.user.id.required");
    }
}