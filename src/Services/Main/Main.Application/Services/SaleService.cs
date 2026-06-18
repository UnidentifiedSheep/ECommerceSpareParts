using Main.Application.Dtos.Sale;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Storage;
using Main.Entities.Sale;

namespace Main.Application.Services;

public class SaleService : ISaleService
{
    public List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<NewSaleContentDto> saleContents)
    {
        return DistributeDetails(
            storageContentValues,
            saleContents.Select(x => (x.ProductId, x.Price, x.PriceWithDiscount, x.Count, x.Comment)));
    }

    public List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<EditSaleContentDto> saleContents)
    {
        return DistributeDetails(
            storageContentValues,
            saleContents.Select(x => (x.ProductId, x.Price, x.PriceWithDiscount, x.Count, x.Comment)));
    }

    private List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<(int productId, decimal price, decimal priceWithDiscount, int count, string? comment)> saleContents)
    {
        var result = new List<SaleContent>();

        var storageByProduct = storageContentValues
            .Select(x =>
            {
                if (x.Count <= 0)
                    throw new InvalidOperationException("Invalid taken quantity");

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

        foreach (var (productId, price, priceWithDiscount, count, comment) in
                 saleContents.OrderByDescending(x => x.count))
        {
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
                    details.Add(SaleContentDetail.Create(
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

            if (leftToDistribute != 0)
                throw new InvalidOperationException("Unable to distribute details");

            var content = SaleContent.Create(
                productId,
                price,
                priceWithDiscount,
                details);
            
            content.SetComment(comment);
            
            result.Add(content);
        }

        return result;
    }
}
