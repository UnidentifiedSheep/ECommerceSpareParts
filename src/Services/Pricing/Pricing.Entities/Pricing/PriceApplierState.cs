using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities.Pricing;

public class PriceApplierState : 
    AuditableEntity<PriceApplierState, PriceApplierStateKey>, 
    ILinqEntity<PriceApplierState, PriceApplierStateKey>
{
    public string PriceApplierSystemName { get; private set; } = null!;
    public PriceApplierUsage Usage { get; private set; }
    public int Order { get; private set; }
    public bool Enabled { get; private set; }

    public static PriceApplierState Create(
        string priceApplierSystemName,
        PriceApplierUsage usage,
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
    
    public void Enable() => Enabled = true;
    public void Disable() => Enabled = false;
    
    public override PriceApplierStateKey GetId() => new(PriceApplierSystemName, Usage);
    public static Expression<Func<PriceApplierState, PriceApplierStateKey>> GetKeySelector()
        => x => new PriceApplierStateKey(x.PriceApplierSystemName, x.Usage);
    public static Expression<Func<PriceApplierState, bool>> GetEqualityExpression(PriceApplierStateKey key)
        => x => x.PriceApplierSystemName == key.PriceApplierSystemName && x.Usage == key.Usage;
}

public readonly struct PriceApplierStateKey(
    string priceApplierSystemName, 
    PriceApplierUsage usage) : ICompositeKey
{
    public string PriceApplierSystemName => priceApplierSystemName;
    public PriceApplierUsage Usage => usage;
    public object[] ToArray() => [priceApplierSystemName, usage];
}