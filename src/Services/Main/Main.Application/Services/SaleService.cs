using System.Text;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;
using Main.Application.Handlers.ProductReservations.UpdateOrganizationReservationCounts;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Models;
using Main.Application.Models.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Enums;
using MediatR;

namespace Main.Application.Services;

public class SaleService(
    ISender sender,
    IProductRepository productRepository
) : ISaleService
{
    public List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<NewSaleContentDto> saleContents)
    {
        return DistributeDetails(
            storageContentValues,
            saleContents.Select((x, index) => new SaleContentInput(
                index,
                x.ProductId,
                x.Price,
                x.PriceWithDiscount,
                x.Count,
                x.Comment)));
    }

    public List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<EditSaleContentDto> saleContents)
    {
        return DistributeDetails(
            storageContentValues,
            saleContents.Select((x, index) => new SaleContentInput(
                index,
                x.ProductId,
                x.Price,
                x.PriceWithDiscount,
                x.Count,
                x.Comment)));
    }

    public Task CheckReservations(
        IEnumerable<NewSaleContentDto> saleContents,
        Guid buyerOrganizationId,
        string storageName,
        bool takeFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken = default)
    {
        return CheckReservations(
            saleContents.Select(x => new SaleContentInput(
                0,
                x.ProductId,
                x.Price,
                x.PriceWithDiscount,
                x.Count,
                x.Comment)),
            buyerOrganizationId,
            storageName,
            takeFromOtherStorages,
            confirmationCode,
            cancellationToken);
    }

    public Task CheckReservations(
        IEnumerable<EditSaleContentDto> saleContents,
        Guid buyerOrganizationId,
        string storageName,
        bool takeFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken = default)
    {
        return CheckReservations(
            saleContents.Select(x => new SaleContentInput(
                0,
                x.ProductId,
                x.Price,
                x.PriceWithDiscount,
                x.Count,
                x.Comment)),
            buyerOrganizationId,
            storageName,
            takeFromOtherStorages,
            confirmationCode,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
        string storageName,
        IEnumerable<NewSaleContentDto> saleContents,
        StorageMovementType movementType,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var contents = saleContents.ToList();
        var takenFromStorage = await TakeFromStorage(
            storageName,
            contents.Select(x => (x.ProductId, x.Count)),
            movementType,
            takeFromOtherStorages,
            cancellationToken);

        return DistributeDetails(takenFromStorage.Contents, contents);
    }

    public async Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
        string storageName,
        IEnumerable<EditSaleContentDto> saleContents,
        StorageMovementType movementType,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var contents = saleContents.ToList();
        var takenFromStorage = await TakeFromStorage(
            storageName,
            contents.Select(x => (x.ProductId, x.Count)),
            movementType,
            takeFromOtherStorages,
            cancellationToken);

        return DistributeDetails(takenFromStorage.Contents, contents);
    }

    public async Task UpdateOrganizationReservationCounts(
        Guid buyerOrganizationId,
        Dictionary<int, int> counts,
        CancellationToken cancellationToken = default)
    {
        var toSubtract = counts
            .Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);

        if (toSubtract.Count == 0) return;

        await sender.Send(
            new UpdateOrganizationReservationCountsCommand(buyerOrganizationId, toSubtract),
            cancellationToken);
    }

    public Task ConsumeOrganizationReservations(
        Sale sale,
        Guid buyerOrganizationId,
        CancellationToken cancellationToken = default)
    {
        var toSubtract = sale.Contents
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

        return UpdateOrganizationReservationCounts(
            buyerOrganizationId,
            toSubtract,
            cancellationToken);
    }

    public async Task RestoreContents(
        Sale sale,
        StorageMovementType movementType,
        CancellationToken cancellationToken = default)
    {
        var toRestore = sale.Contents
            .SelectMany(content => content.Details
                .Select(detail => new RestoreContentItem(detail, content.ProductId)))
            .ToList();

        if (toRestore.Count == 0) return;

        await sender.Send(new RestoreContentCommand(toRestore, movementType), cancellationToken);
    }

    private List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<SaleContentInput> saleContents)
    {
        var result = new List<(int OriginalIndex, SaleContent Content)>();

        var storageByProduct = storageContentValues
            .Select(x =>
            {
                if (x.Count <= 0) throw new InvalidOperationException("Invalid taken quantity");

                return new
                {
                    x.ProductId,
                    Detail = SaleContentDetail.Create(
                        x.Id,
                        x.CurrencyId,
                        x.BuyPrice,
                        x.Count,
                        x.PurchaseDatetime)
                };
            })
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.Detail)
                    .OrderByDescending(d => d.Count)
                    .ThenByDescending(d => d.BuyPrice)
                    .ToList()
            );

        foreach (var contentInput in saleContents.OrderByDescending(x => x.Count))
        {
            var (originalIndex, productId, price, priceWithDiscount, count, comment) = contentInput;
            if (!storageByProduct.TryGetValue(productId, out var storage))
                throw new InvalidOperationException($"No storage for product {productId}");

            var details = new List<SaleContentDetail>();
            var leftToDistribute = count;

            var i = 0;

            while (leftToDistribute > 0 && i < storage.Count)
            {
                var detail = storage[i];

                if (leftToDistribute >= detail.Count)
                {
                    details.Add(detail);
                    leftToDistribute -= detail.Count;
                    storage.RemoveAt(i);
                }
                else
                {
                    details.Add(
                        SaleContentDetail.Create(
                            detail.StorageContentId,
                            detail.CurrencyId,
                            detail.BuyPrice,
                            leftToDistribute,
                            detail.PurchaseDatetime));

                    storage[i] = SaleContentDetail.Create(
                        detail.StorageContentId,
                        detail.CurrencyId,
                        detail.BuyPrice,
                        detail.Count - leftToDistribute,
                        detail.PurchaseDatetime);

                    leftToDistribute = 0;
                }
            }

            if (leftToDistribute != 0) throw new InvalidOperationException("Unable to distribute details");

            var content = SaleContent.Create(
                productId,
                price,
                priceWithDiscount,
                details);

            content.SetComment(comment);

            result.Add((originalIndex, content));
        }

        return result
            .OrderBy(x => x.OriginalIndex)
            .Select(x => x.Content)
            .ToList();
    }

    private async Task<SubtractStorageContentsResult> TakeFromStorage(
        string storageName,
        IEnumerable<(int ProductId, int Count)> contents,
        StorageMovementType movementType,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken)
    {
        return await sender.Send(
            new SubtractStorageContentsCommand(
                contents.Select(x =>
                    new SubtractProductFromStorageItem(
                        x.ProductId,
                        storageName,
                        x.Count,
                        takeFromOtherStorages)),
                movementType),
            cancellationToken);
    }

    private async Task CheckReservations(
        IEnumerable<SaleContentInput> saleContents,
        Guid buyerOrganizationId,
        string storageName,
        bool takeFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken)
    {
        var contentList = saleContents.ToList();
        var (byReservation, byStock) = await GetStockReservations(
            contentList,
            buyerOrganizationId,
            storageName,
            takeFromOtherStorages,
            cancellationToken);

        if (byStock.Count != 0) throw new NotEnoughCountOnStorageException(byStock.Keys);

        if (byReservation.Count == 0) return;

        var criteria = Criteria<Product>.New()
            .Track(false)
            .Include(x => x.Producer)
            .Where(x => byReservation.Keys.Contains(x.Id))
            .Build();

        var products = (await productRepository.ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.Id);

        var res = new Dictionary<string, int>();
        var codeBuilder = new StringBuilder();
        foreach (var (id, count) in byReservation.OrderBy(x => x.Key))
        {
            var art = products[id];
            var key = $"{art.Producer.Name}_{art.Sku.NormalizedValue}";
            res[key] = count;
            codeBuilder.Append($"{key}:{count}");
        }

        var currentCode = codeBuilder.ToString();
        if (currentCode != confirmationCode) throw new SaleSoftConfirmationNeededException(currentCode, res);
    }

    private async Task<(Dictionary<int, int> byReservation, Dictionary<int, int> byStock)>
        GetStockReservations(
            IEnumerable<SaleContentInput> saleContents,
            Guid buyerOrganizationId,
            string storageName,
            bool takeFromOtherStorages,
            CancellationToken cancellationToken = default)
    {
        var neededProductCounts = saleContents
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(z => z.Count));

        var result = await sender.Send(
            new GetProductsWithNotEnoughStockQuery(
                buyerOrganizationId,
                storageName,
                takeFromOtherStorages,
                neededProductCounts),
            cancellationToken);

        return (result.NotEnoughByReservation, result.NotEnoughByStock);
    }

    private sealed record SaleContentInput(
        int OriginalIndex,
        int ProductId,
        decimal Price,
        decimal PriceWithDiscount,
        int Count,
        string? Comment
    );
}
