using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence.Repositories;

public class PriceOfferRefreshStateRepository(
    DContext context, 
    IQueryableExtensions extensions
) : LinqRepositoryBase<DContext, PriceOfferRefreshState, PriceOfferRefreshStateKey>(context, extensions), IPriceOfferRefreshStateRepository
{
    public async Task<bool> UpsertStateAsync(
        PriceOfferRefreshState model,
        CancellationToken cancellationToken = default)
    {
        var affected = await Context.Database.ExecuteSqlAsync(
            $"""
             INSERT INTO public.price_offer_refresh_states (
                 product_id,
                 source,
                 storage_name,
                 last_offers_updated_at,
                 last_offers_count
             )
             VALUES (
                 {model.ProductId},
                 {model.Source.ToString()},
                 {model.StorageName},
                 {model.LastOffersUpdatedAt},
                 {model.LastOffersCount}
             )
             ON CONFLICT (product_id, source, storage_name)
             DO UPDATE SET
                 last_offers_updated_at = EXCLUDED.last_offers_updated_at,
                 last_offers_count = EXCLUDED.last_offers_count
             WHERE price_offer_refresh_states.last_offers_updated_at IS NULL
                OR price_offer_refresh_states.last_offers_updated_at < EXCLUDED.last_offers_updated_at;
             """, 
            cancellationToken);

        return affected > 0;
    }
}
