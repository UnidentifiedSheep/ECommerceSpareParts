using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Sales;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Sales;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Pricing;
using Main.Core.Interfaces.Services;
using Main.Core.Models;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Sales.EditSale;

[Transactional]
[ExceptionType<CurrencyNotFoundException>]
[ExceptionType<UserNotFoundException>]
[ExceptionType<SaleNotFoundException>]
[ExceptionType<SaleContentNotFoundException>]
public record EditSaleCommand(
    IEnumerable<EditSaleContentDto> EditedContent,
    IEnumerable<PrevAndNewValue<StorageContent>> StorageContentValues,
    Dictionary<int, List<SaleContentDetail>> MovedToStorage,
    string SaleId,
    int CurrencyId,
    Guid UpdatedUserId,
    DateTime SaleDateTime,
    string? Comment) : ICommand;

public class EditSaleHandler(
    DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork,
    ISaleService saleService,
    ISaleRepository saleRepository,
    IPriceGenerator priceGenerator) : ICommandHandler<EditSaleCommand>
{
    public async Task<Unit> Handle(EditSaleCommand request, CancellationToken cancellationToken)
    {
        var updatedUserId = request.UpdatedUserId;
        var saleId = request.SaleId;
        var editedContent = request.EditedContent;
        var movedToStorage = request.MovedToStorage;
        await ValidateData(updatedUserId, request.CurrencyId, cancellationToken);

        var sale = await saleRepository.GetSaleForUpdate(saleId, true, cancellationToken)
                   ?? throw new SaleNotFoundException(saleId);

        sale.Comment = request.Comment;
        sale.SaleDatetime = request.SaleDateTime;
        sale.UpdatedUserId = updatedUserId;
        sale.UpdateDatetime = DateTime.Now;

        var saleContents = (await saleRepository.GetSaleContentsForUpdate(saleId, true, cancellationToken))
            .ToDictionary(x => x.Id);
        var saleContentDetails = (await saleRepository.GetSaleContentDetailsForUpdate(saleContents.Keys,
            true, cancellationToken)).ToDictionary(x => x.Id);

        var detailGroups = saleService.GetDetailsGroup(request.StorageContentValues);
        var deletedContentIds = new HashSet<int>(saleContents.Keys);

        foreach (var item in editedContent)
            if (item.Id != null)
                WhenSaleContentExists(item, deletedContentIds, saleContents, detailGroups, movedToStorage,
                    saleContentDetails);
            else
                WhenSaleContentNotExists(item, sale, detailGroups);

        if (detailGroups.Any(x => x.Value.Count > 0))
            throw new ArgumentException("Несовпадение количества в деталях и продажах");

        var deletedContents = saleContents
            .Where(kvp => deletedContentIds.Contains(kvp.Key))
            .Select(x => x.Value)
            .ToList();
        unitOfWork.RemoveRange(deletedContents);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void WhenSaleContentNotExists(EditSaleContentDto item, Sale sale,
        Dictionary<int, Queue<SaleContentDetail>> detailGroups)
    {
        var saleContent = item.Adapt<SaleContent>();
        sale.SaleContents.Add(saleContent);
        if (!detailGroups.TryGetValue(item.ArticleId, out var queue))
            throw new ArgumentException($"Нет деталей по артикулу {item.ArticleId}");

        var counter = item.Count;
        AssignDetailsToContent(saleContent, queue, counter);
    }

    private void WhenSaleContentExists(EditSaleContentDto item, HashSet<int> deletedContentIds,
        Dictionary<int, SaleContent> saleContents,
        Dictionary<int, Queue<SaleContentDetail>> detailGroups, Dictionary<int, List<SaleContentDetail>> movedToStorage,
        Dictionary<int, SaleContentDetail> saleContentDetails)
    {
        deletedContentIds.Remove(item.Id!.Value);
        if (!saleContents.TryGetValue(item.Id.Value, out var saleContent))
            throw new SaleContentNotFoundException(item.Id.Value);

        saleContent.Discount = priceGenerator.GetDiscountFromPrices(item.PriceWithDiscount, item.Price);
        saleContent.Price = item.PriceWithDiscount;
        saleContent.TotalSum = item.PriceWithDiscount * item.Count;

        if (saleContent.Count < item.Count)
        {
            if (!detailGroups.TryGetValue(item.ArticleId, out var queue))
                throw new ArgumentException($"Нет деталей по артикулу {item.ArticleId}");

            var counter = item.Count - saleContent.Count;
            AssignDetailsToContent(saleContent, queue, counter);
        }
        else if (saleContent.Count > item.Count)
        {
            ReturnDetailsToStorage(movedToStorage[item.Id.Value], saleContentDetails);
        }

        saleContent.Count = item.Count;
    }

    private void ReturnDetailsToStorage(IEnumerable<SaleContentDetail> movedDetails,
        Dictionary<int, SaleContentDetail> saleContentDetails)
    {
        foreach (var tempDetail in movedDetails)
        {
            var realDetail = saleContentDetails[tempDetail.Id];
            if (realDetail.Count < tempDetail.Count)
                throw new ArgumentException("В продаже нет достаточного количества элементов для возврата на склад");
            realDetail.Count -= tempDetail.Count;
            if (realDetail.Count == 0)
                unitOfWork.Remove(realDetail);
        }
    }

    private void AssignDetailsToContent(SaleContent saleContent, Queue<SaleContentDetail> queue, int requiredCount)
    {
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
            throw new ArgumentException($"Недостаточно деталей для артикула {saleContent.ArticleId}");
    }

    private async Task ValidateData(Guid userId, int currencyId, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureUserExists(userId)
            .EnsureCurrencyExists(currencyId);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}