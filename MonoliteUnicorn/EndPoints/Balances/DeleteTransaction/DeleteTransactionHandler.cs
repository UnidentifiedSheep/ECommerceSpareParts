using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;

namespace MonoliteUnicorn.EndPoints.Balances.DeleteTransaction;

public record DeleteTransactionCommand(string TransactionId, string WhoDeleteUserId) : ICommand<Unit>;

public class DeleteTransactionValidation : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionValidation()
    {
        RuleFor(x => x.TransactionId).NotEmpty()
            .WithMessage("Id Транзакции не может быть пуст.");
        RuleFor(x => x.WhoDeleteUserId).NotEmpty()
            .WithMessage("Id пользователя который удаляет транзакцию не может быть пуст.");
    }
}
public class DeleteTransactionHandler(IBalance balance, DContext context) : ICommandHandler<DeleteTransactionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken) ?? throw new TransactionNotFount(request.TransactionId);
        if (transaction.Status != nameof(TransactionStatus.Normal)) throw new BadTransactionStatusException(transaction.Status);
        await balance.DeleteTransaction(request.TransactionId, request.WhoDeleteUserId, cancellationToken);
        return Unit.Value;
    }
}