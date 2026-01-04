using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Balances;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Purchase;
using Main.Core.Entities;
using Main.Core.Enums;
using Mapster;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[Transactional]
public record CreatePurchaseCommand(
    IEnumerable<NewPurchaseContentDto> Content,
    int CurrencyId,
    string? Comment,
    Guid CreatedUserId,
    Guid TransactionId,
    string StorageName,
    Guid SupplierId,
    DateTime PurchaseDateTime) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(Purchase Purchase);

public class CreatePurchaseHandler(DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var content = request.Content.ToList();
        var whoCreated = request.CreatedUserId;
        var supplierId = request.SupplierId;
        var currencyId = request.CurrencyId;
        var storageName = request.StorageName;
        var transactionId = request.TransactionId;
        var articleIds = content.Select(x => x.ArticleId).ToHashSet();

        await ValidateData([whoCreated, supplierId], currencyId, storageName, transactionId, articleIds,
            cancellationToken);

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

    private async Task ValidateData(IEnumerable<Guid> userIds, int currencyId, string storageName,
        Guid transactionId, IEnumerable<int> articleIds, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureUserExists(userIds)
            .EnsureCurrencyExists(currencyId)
            .EnsureTransactionExists(transactionId)
            .EnsureStorageExists(storageName)
            .EnsureArticleExists(articleIds);
        
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}