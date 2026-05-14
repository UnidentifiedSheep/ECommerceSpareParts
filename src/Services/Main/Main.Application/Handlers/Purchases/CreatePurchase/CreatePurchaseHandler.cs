using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[Transactional, AutoSave]
public record CreatePurchaseCommand(
    IEnumerable<(NewPurchaseContentDto content, int storageContentId)> Content,
    int CurrencyId,
    Guid SupplierId,
    Guid TransactionId,
    string StorageName,
    DateTime PurchaseDateTime,
    string? Comment) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(Purchase Purchase);

public class CreatePurchaseHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = Purchase.Create(
            request.SupplierId,
            request.CurrencyId,
            request.TransactionId,
            request.StorageName,
            request.PurchaseDateTime);

        purchase.SetComment(request.Comment);

        foreach (var (content, storageContentId) in request.Content)
            purchase.AddContent(PurchaseContent.Create(
                content.ProductId,
                content.Count,
                content.Price,
                storageContentId,
                content.Comment));

        await unitOfWork.AddAsync(purchase, cancellationToken);
        return new CreatePurchaseResult(purchase);
    }
}