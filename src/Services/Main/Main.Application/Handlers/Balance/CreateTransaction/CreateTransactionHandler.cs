using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Transaction;
using Main.Enums;

namespace Main.Application.Handlers.Balance.CreateTransaction;

[Transactional(IsolationLevel.Serializable, 20, 3)]
public record CreateTransactionCommand(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime,
    TransactionStatus TransactionStatus) : ICommand<CreateTransactionResult>;

public record CreateTransactionResult(Transaction Transaction);

public class CreateTransactionHandler(
    ITransactionRepository transactionRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var senderId = request.SenderId;
        var receiverId = request.ReceiverId;
        var amount = request.Amount;
        var currencyId = request.CurrencyId;
        var transactionDateTime = request.TransactionDateTime;

        var criteria = Criteria<Transaction>.New()
            .ForUpdate()
            .Build();

        var prevSenderTransaction = await transactionRepository.GetPreviousTransactionAsync(transactionDateTime, 
            senderId, currencyId, criteria, cancellationToken);
        var prevReceiverTransaction = await transactionRepository.GetPreviousTransactionAsync(transactionDateTime,
            receiverId, currencyId, criteria, cancellationToken);

        Transaction transaction = Transaction.Create(senderId, receiverId, currencyId, request.TransactionStatus,
            amount, prevSenderTransaction, prevReceiverTransaction, transactionDateTime);

        await unitOfWork.AddAsync(transaction, cancellationToken);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        await balanceService.RecalculateBalanceAsync(transaction, null, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateTransactionResult(transaction);
    }
}