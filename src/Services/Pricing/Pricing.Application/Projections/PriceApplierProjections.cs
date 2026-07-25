using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Entities.Pricing;

namespace Pricing.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class PriceApplierStateDtoProjectionProvider
    : IProjectionProvider<PriceApplierState, PriceApplierStateDto>
{
    public Expression<Func<PriceApplierState, PriceApplierStateDto>> Projection { get; } =
        x => new PriceApplierStateDto
        {
            Enabled = x.Enabled,
            Usage = x.Usage,
            Order = x.Order,
            PriceApplierSystemName = x.PriceApplierSystemName
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class PriceApplierDtoProjectionProvider
    : IProjectionProvider<PriceApplier, PriceApplierDto>
{
    public PriceApplierDtoProjectionProvider(
        IProjectionProvider<PriceApplierState, PriceApplierStateDto> stateProjection)
    {
        var stateToDto = stateProjection.Projection;

        Projection = x => new PriceApplierDto
        {
            SystemName = x.SystemName,
            Name = x.Name ?? x.SystemName,
            IsDynamic = x.DslLogic != null,
            DslLogic = x.DslLogic,
            States = x.States.Select(z => stateToDto.Invoke(z)).ToList()
        };
    }

    public Expression<Func<PriceApplier, PriceApplierDto>> Projection { get; }
}
