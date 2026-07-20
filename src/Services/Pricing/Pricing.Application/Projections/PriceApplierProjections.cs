using System.Linq.Expressions;
using LinqKit;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Entities.Pricing;

namespace Pricing.Application.Projections;

public static class PriceApplierProjections
{
    public static readonly Expression<Func<PriceApplier, PriceApplierDto>> ToApplierDto
        = x => new PriceApplierDto
        {
            SystemName = x.SystemName,
            DslLogic = x.DslLogic,
            States = x.States.Select(z => ToStateDto.Invoke(z)).ToList()
        };
    
    public static readonly Expression<Func<PriceApplierState, PriceApplierStateDto>> ToStateDto
        = x => new PriceApplierStateDto
        {
            Enabled = x.Enabled,
            Usage = x.Usage,
            Order = x.Order,
            PriceApplierSystemName = x.PriceApplierSystemName
        };
}