using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Balance.EditTransaction;

public class EditTransactionValidation : AbstractValidator<EditTransactionCommand>
{
    public EditTransactionValidation()
    {
        RuleFor(x => x.Amount)
            .SetValidator(new TransactionAmountValidator());

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithLocalizationKey("transaction.id.required");
    }
}