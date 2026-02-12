using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Balance.CreateTransaction;

public class CreateTransactionValidation : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(command => command.SenderId).NotEmpty().WithMessage("Поле 'SenderId' должно быть заполнено");
        RuleFor(command => command.ReceiverId).NotEmpty().WithMessage("Поле 'ReceiverId' должно быть заполнено");
        RuleFor(command => command.Amount).SetValidator(new TransactionAmountValidator());
        RuleFor(command => command.TransactionDateTime.ToUniversalTime())
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMonths(-2))
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("Дата транзакции не может отличаться более чем на 2 месяца от текущей даты");
        RuleFor(command => command.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}