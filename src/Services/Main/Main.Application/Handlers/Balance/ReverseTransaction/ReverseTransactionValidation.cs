using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

public class ReverseTransactionValidation : AbstractValidator<ReverseTransactionCommand>
{
    public ReverseTransactionValidation()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithLocalizationKey("transaction.id.required");
    }
}