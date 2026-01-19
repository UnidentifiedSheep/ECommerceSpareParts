using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Entities;
using Mapster;

namespace Main.Application.Services;

public class SaleService : ISaleService
{
    public Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues)
    {
        return storageContentValues.Select(x =>
            {
                var taken = x.Prev.Count - x.NewValue.Count;
                if (taken <= 0 || taken > x.Prev.Count)
                    throw new ArgumentException("Некорректное taken количество");
                if (x.Prev.Id != x.NewValue.Id)
                    throw new ArgumentException("Не совпадает Id в старом и новом значении");
                var detail = x.NewValue.Adapt<SaleContentDetail>();
                detail.Count = taken;

                return (x.Prev.ArticleId, Detail: detail);
            })
            .GroupBy(x => x.ArticleId)
            .ToDictionary(
                g => g.Key,
                g => new Queue<SaleContentDetail>(
                    g.Select(x => x.Detail)
                        .OrderByDescending(x => x.Count)
                        .ThenByDescending(x => x.BuyPrice)
                )
            );
    }
}