using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Balance.EditTransaction;

public class EditTransactionValidation : AbstractValidator<EditTransactionCommand>
{
    public EditTransactionValidation()
    {
        RuleFor(x => x.Amount)
            .SetValidator(new TransactionAmountValidator());

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Id транзакции не может быть пустым");
    }
}