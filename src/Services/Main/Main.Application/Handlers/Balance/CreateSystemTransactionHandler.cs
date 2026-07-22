using Abstractions.Models.Options;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Balance;

public record CreateSystemTransactionCommand(
    Guid OrganizationId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime,
    SystemTransactionDirection Direction,
    bool ForcePayment = false
) : ICommand<CreateTransactionResult>;

public class CreateSystemTransactionHandler(
    ISender sender,
    IOptions<SystemOptions> systemOptions
) : ICommandHandler<CreateSystemTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateSystemTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var systemOrganizationId = systemOptions.Value.SystemId;

        var senderId = request.Direction == SystemTransactionDirection.UserToSystem
            ? request.OrganizationId
            : systemOrganizationId;

        var receiverId = request.Direction == SystemTransactionDirection.UserToSystem
            ? systemOrganizationId
            : request.OrganizationId;

        return await sender.Send(
            new CreateTransactionCommand(
                senderId,
                receiverId,
                request.Amount,
                request.CurrencyId,
                request.TransactionDateTime,
                TransactionSourceType.Manual,
                TransactionCreationMode.System,
                request.ForcePayment),
            cancellationToken);
    }
}
