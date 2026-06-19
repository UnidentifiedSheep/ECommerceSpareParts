using Application.Common.Interfaces.Cqrs;
using Application.Common.Models.Options;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Balance;

public record CreateSystemTransactionCommand(
    Guid UserId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime,
    SystemTransactionDirection Direction) : ICommand<CreateTransactionResult>;

public class CreateSystemTransactionHandler(
    ISender sender,
    IOptions<SystemOptions> systemOptions) : ICommandHandler<CreateSystemTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateSystemTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var systemUserId = systemOptions.Value.SystemId;

        var senderId = request.Direction == SystemTransactionDirection.UserToSystem
            ? request.UserId
            : systemUserId;

        var receiverId = request.Direction == SystemTransactionDirection.UserToSystem
            ? systemUserId
            : request.UserId;

        return await sender.Send(
            new CreateTransactionCommand(
                senderId,
                receiverId,
                request.Amount,
                request.CurrencyId,
                request.TransactionDateTime,
                TransactionSourceType.Manual,
                TransactionCreationMode.System),
            cancellationToken);
    }
}
