using System.Linq.Expressions;
using Abstractions;
using Application.Common.Extensions;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Configuration;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortBy.Value.MapDefault<ProductPriceOption, decimal>(x => x.Score, true)
            .Map<ProductPriceOption, decimal>("score", x => x.Score);
        
        QueryableSortBy.Value.ConfigureForJob();
    }
}