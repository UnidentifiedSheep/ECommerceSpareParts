using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Services.Balances;

namespace MonoliteUnicorn.EndPoints.Balances.CreateTransaction;

public record CreateTransactionCommand(string SenderId, string ReceiverId, decimal Amount, int CurrencyId, 
    string WhoCreatedTransaction, DateTime TransactionDateTime) : ICommand<Unit>;

public class CreateTransactionValidation : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidation()
    {
        RuleFor(command => command.SenderId).NotEmpty().WithMessage("Поле 'SenderId' должно быть заполнено");
        RuleFor(command => command.ReceiverId).NotEmpty().WithMessage("Поле 'ReceiverId' должно быть заполнено");
        RuleFor(command => command.Amount).GreaterThan(0).WithMessage("Сумма транзакции должна больше 0");
        RuleFor(command => command.TransactionDateTime)
            .GreaterThanOrEqualTo(DateTime.Now.AddMonths(-2))
            .LessThanOrEqualTo(DateTime.Now.AddMonths(2))
            .WithMessage("Дата транзакции не может отличаться более чем на 2 месяца от текущей даты");
    }
}

public class CreateTransactionHandler(IBalance balance) : ICommandHandler<CreateTransactionCommand, Unit>
{
    public async Task<Unit> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        await balance.CreateTransactionAsync(request.SenderId, request.ReceiverId, request.Amount, 
            TransactionStatus.Normal, request.CurrencyId, request.WhoCreatedTransaction, 
            request.TransactionDateTime, cancellationToken);
        return Unit.Value;
    }
}