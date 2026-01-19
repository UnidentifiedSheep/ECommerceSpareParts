using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[Transactional]
public record CreatePurchaseCommand(IEnumerable<NewPurchaseContentDto> Content, int CurrencyId, string? Comment,
    Guid CreatedUserId, Guid TransactionId, string StorageName, Guid SupplierId,
    DateTime PurchaseDateTime) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(Purchase Purchase);

public class CreatePurchaseHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var content = request.Content.ToList();
        var whoCreated = request.CreatedUserId;
        var supplierId = request.SupplierId;
        var currencyId = request.CurrencyId;
        var storageName = request.StorageName;
        var transactionId = request.TransactionId;

        var purchaseContents = content.Select(x => x.Adapt<PurchaseContent>()).ToList();
        var purchaseModel = new Purchase
        {
            CurrencyId = currencyId,
            Comment = request.Comment,
            Storage = storageName,
            CreatedUserId = whoCreated,
            SupplierId = supplierId,
            PurchaseDatetime = request.PurchaseDateTime,
            PurchaseContents = purchaseContents,
            TransactionId = transactionId,
            State = PurchaseState.Draft
        };
        await unitOfWork.AddAsync(purchaseModel, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePurchaseResult(purchaseModel);
    }
}