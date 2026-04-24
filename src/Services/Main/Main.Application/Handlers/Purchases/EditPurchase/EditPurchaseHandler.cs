using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;
using Mapster;

namespace Main.Application.Handlers.Purchases.EditPurchase;

[Transactional]
public record EditPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    string PurchaseId,
    int CurrencyId,
    string? Comment,
    Guid UpdatedUserId,
    DateTime PurchaseDateTime) : ICommand<EditPurchaseResult>;

/// <param name="EditedCounts">
///     Словарь где Key - айди артикула,
///     далее словарь где Key - цена и Value - количество.
///     Если количество отрицательное, то это количество, которое взяли со склада.
///     Если положительное, то вернули на склад.
/// </param>
public record EditPurchaseResult(Dictionary<int, Dictionary<decimal, int>> EditedCounts);

public class EditPurchaseHandler(IPurchaseRepository purchaseRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditPurchaseCommand, EditPurchaseResult>
{
    public async Task<EditPurchaseResult> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var currencyId = request.CurrencyId;
        var purchaseDateTime = request.PurchaseDateTime;
        var whoUpdated = request.UpdatedUserId;
        var comment = request.Comment;
        var result = new Dictionary<int, Dictionary<decimal, int>>();
        var content = request.Content.ToList();

        var purchase = await GetPurchase(purchaseId, cancellationToken);

        var purchaseContents = await GetPurchaseContents(purchaseId, cancellationToken);

        var existingIds = content.Where(x => x.Id != null)
            .Select(x => x.Id!.Value).ToHashSet();
        var toDelete = purchaseContents.Values
            .Where(x => !existingIds.Contains(x.Id)).ToList();

        var allArticleIds = content.Select(x => x.ArticleId)
            .Concat(purchaseContents.Values.Select(x => x.ProductId))
            .Distinct();

        foreach (var articleId in allArticleIds)
            result[articleId] = new Dictionary<decimal, int>();

        foreach (var item in toDelete)
            if (!result[item.ProductId].TryAdd(item.Price, -item.Count))
                result[item.ProductId][item.Price] += -item.Count;

        SetFields(purchase, currencyId, purchaseDateTime, comment, whoUpdated);

        var contentToAdd = new List<PurchaseContent>();

        foreach (var item in content)
        {
            PurchaseContent? existingContent = null;
            if (item.Id != null && !purchaseContents.TryGetValue(item.Id.Value, out existingContent))
                throw new PurchaseContentNotFoundException(item.Id.Value);
            if (item.Id == null)
            {
                if (!result[item.ArticleId].TryAdd(item.Price, item.Count))
                    result[item.ArticleId][item.Price] += item.Count;
                existingContent = item.Adapt<PurchaseContent>();
                contentToAdd.Add(existingContent);
            }
            else
            {
                if (item.ArticleId != existingContent!.ProductId)
                    throw new ArticleDoesntMatchContentException(item.ArticleId);
                var delta = item.Count - existingContent.Count;
                item.Adapt(existingContent);
                if (delta == 0) continue;
                if (!result[item.ArticleId].TryAdd(item.Price, delta))
                    result[item.ArticleId][item.Price] += delta;
            }

            existingContent.PurchaseId = purchaseId;
        }

        foreach (var kv in result.ToList())
        {
            foreach (var price in kv.Value.Where(x => x.Value == 0).Select(x => x.Key))
                kv.Value.Remove(price);

            if (kv.Value.Count == 0) result.Remove(kv.Key);
        }

        await unitOfWork.AddRangeAsync(contentToAdd, cancellationToken);
        unitOfWork.RemoveRange(toDelete);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new EditPurchaseResult(result);
    }

    private async Task<Purchase> GetPurchase(string id, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<Purchase, string>() { Data = id }
            .WithTracking()
            .WithForUpdate();
        return await purchaseRepository.GetPurchase(
            options,
            cancellationToken) ?? throw new PurchaseNotFoundException(id);
    }

    private async Task<Dictionary<int, PurchaseContent>> GetPurchaseContents(
        string id, 
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<PurchaseContent, string>() { Data = id }
            .WithTracking()
            .WithForUpdate();
        return (await purchaseRepository
                .GetPurchaseContent(options, cancellationToken))
            .ToDictionary(x => x.Id);
    }

    private void SetFields(
        Purchase purchase,
        int currencyId,
        DateTime purchaseDateTime,
        string? comment,
        Guid updatedUserId)
    {
        purchase.Comment = comment?.Trim();
        purchase.CurrencyId = currencyId;
        purchase.PurchaseDatetime = purchaseDateTime;
        purchase.UpdateDatetime = DateTime.UtcNow;
        purchase.UpdatedUserId = updatedUserId;
    }
}