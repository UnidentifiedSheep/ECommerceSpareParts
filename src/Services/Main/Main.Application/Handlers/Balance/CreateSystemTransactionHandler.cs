using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Entities.Setting;
using Main.Enums.Balances;
using MediatR;

namespace Main.Application.Handlers.Balance;

public record CreateSystemTransactionCommand(
    Guid UserId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime,
    SystemTransactionDirection Direction) : ICommand<CreateTransactionResult>;

public class CreateSystemTransactionHandler(
    ISender sender,
    ISettingsService settingsService) : ICommandHandler<CreateSystemTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateSystemTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var systemUserId = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;

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
