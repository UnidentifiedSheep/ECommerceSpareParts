using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Pricing.Entities.DomainEvents;
using Pricing.Enums;

namespace Pricing.Entities.Pricing;

public class PriceApplierState : 
    AuditableEntity<PriceApplierState, PriceApplierStateKey>, 
    ILinqEntity<PriceApplierState, PriceApplierStateKey>
{
    public string PriceApplierSystemName { get; private set; } = null!;
    public PriceOfferSourceType Usage { get; private set; }
    public int Order { get; private set; }
    public bool Enabled { get; private set; }

    public static PriceApplierState Create(
        string priceApplierSystemName,
        PriceOfferSourceType usage,
        int order,
        bool enabled = true)
        => new()
        {
            Enabled = enabled,
            Usage = usage,
            Order = order,
            PriceApplierSystemName = priceApplierSystemName
                .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("Price applier system name cannot be empty"))
        };

    public void Update(int order, bool enabled = true)
    {
        Order = order;
        Enabled = enabled;
    }

    public override void OnCreated() => AddPriceApplierUpdatedDomainEvent();

    public override void OnUpdated() => AddPriceApplierUpdatedDomainEvent();

    public override void OnDeleted() => AddPriceApplierUpdatedDomainEvent();

    private void AddPriceApplierUpdatedDomainEvent()
        => AddDomainEvent(new PriceApplierUpdatedDomainEvent
        {
            SystemName = PriceApplierSystemName
        });
    
    public override PriceApplierStateKey GetId() => new(PriceApplierSystemName, Usage);
    public static Expression<Func<PriceApplierState, PriceApplierStateKey>> GetKeySelector()
        => x => new PriceApplierStateKey(x.PriceApplierSystemName, x.Usage);
    public static Expression<Func<PriceApplierState, bool>> GetEqualityExpression(PriceApplierStateKey key)
        => x => x.PriceApplierSystemName == key.PriceApplierSystemName && x.Usage == key.Usage;
}

public readonly struct PriceApplierStateKey(
    string priceApplierSystemName, 
    PriceOfferSourceType usage) : ICompositeKey
{
    public string PriceApplierSystemName => priceApplierSystemName;
    public PriceOfferSourceType Usage => usage;
    public object[] ToArray() => [priceApplierSystemName, usage];
}
