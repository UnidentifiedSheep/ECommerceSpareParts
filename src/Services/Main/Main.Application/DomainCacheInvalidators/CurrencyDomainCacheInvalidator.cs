using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Abstractions;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Currency;

namespace Main.Application.CacheInvalidators;

public class CurrencyDomainCacheInvalidator(
    IRelatedDataRepository<Currency> relatedDataRepository, 
    ICache cache,
    IRepository<Currency, int> repository) 
    : DomainCacheInvalidatorBase<Currency, int>(relatedDataRepository, cache)
{
    public override async Task Invalidate(int key)
    {
        var criteria = Criteria<Currency>.New()
            .Where(x => x.Id < key)
            .OrderByDesc(x => x.Id)
            .Track(false)
            .Build();
        
        var prevCurrency = await repository.FirstOrDefaultAsync(criteria);
        if (prevCurrency == null) return;
        var relatedKeys = (await RelatedDataRepository.GetRelatedDataKeys(prevCurrency.Id.ToString()))
            .ToList();
        await Cache.DeleteAsync(relatedKeys);
    }
}