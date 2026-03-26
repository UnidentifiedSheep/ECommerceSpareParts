using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Balance.CreateTransaction;

public class CreateTransactionValidation : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(command => command.SenderId)
            .NotEmpty()
            .WithLocalizationKey("transaction.sender.id.required");

        RuleFor(command => command.ReceiverId)
            .NotEmpty()
            .WithLocalizationKey("transaction.receiver.id.required");

        RuleFor(command => command.Amount)
            .SetValidator(new TransactionAmountValidator());

        RuleFor(command => command.TransactionDateTime.ToUniversalTime())
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMonths(-2))
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithLocalizationKey("transaction.date.out.of.range");

        RuleFor(command => command.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}