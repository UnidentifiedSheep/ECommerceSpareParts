using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Entities;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Mapster;

namespace Main.Application.Services;

public class SaleService : ISaleService
{
    public Dictionary<int, List<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues)
    {
        return storageContentValues.Select(x =>
            {
                var taken = x.Prev.Count - x.NewValue.Count;
                if (taken <= 0 || taken > x.Prev.Count)
                    throw new ArgumentException("Некорректное taken количество");
                if (x.Prev.Id != x.NewValue.Id)
                    throw new ArgumentException("Не совпадает Id в старом и новом значении");
                var prev = x.Prev;
                var detail = SaleContentDetail.Create(prev.Id, prev.CurrencyId, prev.BuyPrice, taken,
                    prev.PurchaseDatetime);

                return (x.Prev.ProductId, Detail: detail);
            })
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.Detail)
                    .OrderByDescending(x => x.Count)
                    .ThenByDescending(x => x.BuyPrice)
                    .ToList()
            );
    }
}