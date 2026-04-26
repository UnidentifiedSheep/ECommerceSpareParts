using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Application.Handlers.Balance.CreateTransaction;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 3)]
public record CreateTransactionCommand(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime) : ICommand<CreateTransactionResult>;

public record CreateTransactionResult(Transaction Transaction);

public class CreateTransactionHandler(
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Transaction transaction = Transaction.Create(
            request.SenderId, 
            request.ReceiverId, 
            request.CurrencyId, 
            TransactionType.Transfer,
            request.Amount, 
            request.TransactionDateTime);
        
        transaction.Complete();
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        
        await unitOfWork.AddAsync(transaction, cancellationToken);
        return new CreateTransactionResult(transaction);
    }
}