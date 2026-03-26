using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

public class DeleteTransactionValidation : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionValidation()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithLocalizationKey("transaction.id.required");

        RuleFor(x => x.WhoDeleteUserId)
            .NotEmpty()
            .WithLocalizationKey("transaction.who.delete.user.id.required");
    }
}