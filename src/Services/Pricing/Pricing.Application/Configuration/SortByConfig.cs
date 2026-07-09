using System.Linq.Expressions;
using Abstractions;
using Application.Common.Extensions;
using Pricing.Entities;

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