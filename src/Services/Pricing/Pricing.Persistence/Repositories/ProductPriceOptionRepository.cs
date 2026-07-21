using Abstractions.Interfaces;
using EFCore.BulkExtensions;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Persistence.Contexts;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Pricing.Persistence.Repositories;

public class ProductPriceOptionRepository(
    DContext context, 
    IUserContext userContext,
    IQueryableExtensions extensions
) : LinqRepositoryBase<DContext, ProductPriceOption, Guid>(context, extensions), IProductPriceOptionRepository
{
    public async Task UpsertAsync(
        IEnumerable<ProductPriceOption> options, 
        CancellationToken cancellationToken = default)
    {
        var optionsList = options.ToList();
        if (optionsList.Count == 0) return;

        optionsList.ForEach(x => x.Touch(userContext.UserIdOrNull));
        
        await Context.BulkInsertOrUpdateAsync(
            optionsList,
            new BulkConfig
            {
                UpdateByProperties =
                [
                    nameof(ProductPriceOption.PriceOfferId)
                ],

                PropertiesToIncludeOnUpdate =
                [
                    nameof(ProductPriceOption.CurrencyId),
                    nameof(ProductPriceOption.Score),
                    nameof(ProductPriceOption.Price),
                    nameof(ProductPriceOption.Markup),
                    nameof(ProductPriceOption.MarkupVersion),
                    nameof(ProductPriceOption.AppliersVersion),
                    nameof(ProductPriceOption.PricingSettingsVersion),
                    nameof(ProductPriceOption.DeliveryTime),
                    nameof(ProductPriceOption.DeliveryProbability),
                    nameof(ProductPriceOption.GuaranteedDeliveryTime)
                ]
            },
            cancellationToken: cancellationToken);
    }
}
