using Application.Common.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Purchase;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Mapster;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[Transactional]
public record CreatePurchaseCommand(
    IEnumerable<NewPurchaseContentDto> Content,
    int CurrencyId,
    string? Comment,
    Guid CreatedUserId,
    string TransactionId,
    string StorageName,
    Guid SupplierId,
    DateTime PurchaseDateTime) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(Purchase Purchase);

public class CreatePurchaseHandler(
    IUserRepository usersRepository,
    ICurrencyRepository currencyRepository,
    IBalanceRepository balanceRepository,
    IStoragesRepository storagesRepository,
    IArticlesRepository articlesRepository,
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
            TransactionId = transactionId
        };
        await unitOfWork.AddAsync(purchaseModel, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePurchaseResult(purchaseModel);
    }

    private async Task ValidateData(IEnumerable<Guid> userIds, int currencyId, string storageName,
        string transactionId, IEnumerable<int> articleIds, CancellationToken cancellationToken = default)
    {
        await usersRepository.EnsureUsersExists(userIds, cancellationToken);
        await currencyRepository.EnsureCurrenciesExists([currencyId], cancellationToken);
        await balanceRepository.EnsureTransactionExists(transactionId, cancellationToken);
        await storagesRepository.EnsureStorageExists(storageName, cancellationToken);
        await articlesRepository.EnsureArticlesExist(articleIds, cancellationToken);
    }
}