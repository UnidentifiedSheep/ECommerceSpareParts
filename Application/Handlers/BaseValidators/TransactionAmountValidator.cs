using FluentValidation;

namespace Application.Handlers.BaseValidators;

public class TransactionAmountValidator : AbstractValidator<decimal>
{
    public TransactionAmountValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .WithMessage("Сумма транзакции должна быть больше 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Сумма транзакции должна иметь максимум 2 числа после запятой.");
    }
}