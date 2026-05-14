using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Purchase;
using Main.Entities.Setting;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record CreateFullPurchaseCommand(
    Guid CreatedUserId,
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class CreateFullPurchaseHandler(
    ISender sender,
    ISettingsService settingsService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateFullPurchaseCommand>
{
    public async Task<Unit> Handle(CreateFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        Guid systemId = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
        
        var transaction = await sender.Send(new CreateTransactionCommand(
            systemId,
            ))
    }

    private async Task<Purchase> CreatePurchase(
        CreateFullPurchaseCommand request,
        Guid transactionId,
        IEnumerable<(NewPurchaseContentDto content, int storageContentId)> contents,
        CancellationToken cancellationToken)
    {
        var purchase = Purchase.Create(
            request.SupplierId,
            request.CurrencyId,
            transactionId,
            request.StorageName,
            request.PurchaseDate);

        purchase.SetComment(request.Comment);

        foreach (var (content, storageContentId) in contents)
            purchase.AddContent(PurchaseContent.Create(
                content.ProductId,
                content.Count,
                content.Price,
                storageContentId,
                content.Comment));

        await unitOfWork.AddAsync(purchase, cancellationToken);
        return purchase;
    }
}