using System.Collections.Immutable;
using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities;
using Main.Entities.Balance;
using Main.Entities.Exceptions.Balances;
using Main.Enums;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 3)]
public record ReverseTransactionCommand(Guid TransactionId, Guid WhoReversed, bool IsSystem = false)
    : ICommand<ReverseTransactionResult>;

public record ReverseTransactionResult(Transaction Transaction);

public class ReverseTransactionHandler(
    ITransactionRepository transactionRepository,
    IBalanceService balanceService) : ICommandHandler<ReverseTransactionCommand, ReverseTransactionResult>
{
    

    public async Task<ReverseTransactionResult> Handle(
        ReverseTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var criteria = Criteria<Transaction>.New()
            .Where(x => x.Id == transactionId)
            .ForUpdate()
            .Track()
            .Build();
        
        var transaction = await transactionRepository.FirstOrDefaultAsync(criteria, cancellationToken)
                          ?? throw new TransactionNotFoundException(transactionId);

        transaction.Reverse(request.WhoReversed);
        await balanceService.ChangeSenderReceiverBalancesAsync(transaction, cancellationToken);
        return new ReverseTransactionResult(transaction);
    }
}