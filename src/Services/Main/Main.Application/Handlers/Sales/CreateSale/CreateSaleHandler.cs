using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Sales;
using Main.Core.Entities;
using Main.Core.Interfaces.Services;
using Main.Core.Models;
using Mapster;

namespace Main.Application.Handlers.Sales.CreateSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateSaleCommand(
    IEnumerable<NewSaleContentDto> SellContent,
    IEnumerable<PrevAndNewValue<StorageContent>> StorageContentValues,
    int CurrencyId,
    Guid BuyerId,
    Guid CreatedUserId,
    string TransactionId,
    string MainStorage,
    DateTime SaleDateTime,
    string? Comment) : ICommand<CreateSaleResult>;

public record CreateSaleResult(Sale Sale);

public class CreateSaleHandler(
    ISaleService saleService,
    DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var buyerId = request.BuyerId;
        var createdUserId = request.CreatedUserId;
        var currencyId = request.CurrencyId;
        var mainStorage = request.MainStorage;

        var saleContentList = request.SellContent.ToList();

        var articleIds = saleContentList.Select(x => x.ArticleId).ToHashSet();

        await ValidateData(transactionId, articleIds, currencyId, buyerId, createdUserId, mainStorage,
            cancellationToken);

        var detailGroups = saleService.GetDetailsGroup(request.StorageContentValues);

        var saleContents = new List<SaleContent>();

        foreach (var item in saleContentList)
        {
            var saleContent = item.Adapt<SaleContent>();
            saleContents.Add(saleContent);

            DistributeDetails(item.ArticleId, item.Count, saleContent, detailGroups);
        }

        if (detailGroups.Any(x => x.Value.Count > 0))
            throw new ArgumentException("Несовпадение количества в деталях и продажах");

        var saleModel = new Sale
        {
            TransactionId = transactionId,
            SaleDatetime = request.SaleDateTime,
            BuyerId = buyerId,
            CreatedUserId = createdUserId,
            Comment = request.Comment,
            SaleContents = saleContents,
            CurrencyId = currencyId,
            MainStorageName = mainStorage
        };

        await unitOfWork.AddAsync(saleModel, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateSaleResult(saleModel);
    }

    private void DistributeDetails(int articleId, int requiredCount, SaleContent saleContent,
        Dictionary<int, Queue<SaleContentDetail>> detailGroups)
    {
        if (!detailGroups.TryGetValue(articleId, out var queue))
            throw new ArgumentException($"Не найдены детали для артикула {articleId}");

        var counter = requiredCount;
        while (counter > 0 && queue.Count > 0)
        {
            var detail = queue.Peek();
            if (detail.Count <= counter)
            {
                counter -= detail.Count;
                saleContent.SaleContentDetails.Add(detail);
                queue.Dequeue();
            }
            else
            {
                var partial = detail.Adapt<SaleContentDetail>();
                partial.Count = counter;
                detail.Count -= counter;
                counter = 0;
                saleContent.SaleContentDetails.Add(partial);
            }
        }

        if (counter > 0)
            throw new ArgumentException($"Недостаточно деталей для артикула {articleId}");
    }

    private async Task ValidateData(string transactionId, IEnumerable<int> articleIds, int currencyId, Guid buyerId,
        Guid createdUserId, string storageName, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureTransactionExists(transactionId)
            .EnsureArticleExists(articleIds)
            .EnsureCurrencyExists(currencyId)
            .EnsureUserExists([buyerId, createdUserId])
            .EnsureStorageExists(storageName);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}