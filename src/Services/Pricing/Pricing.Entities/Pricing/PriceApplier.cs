using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities.Pricing;

public class PriceApplier : 
    AuditableEntity<PriceApplier, string>, 
    ILinqEntity<PriceApplier, string>
{
    public string SystemName { get; private set; } = null!;
    public string? Name { get; private set; }
    public string? DslLogic { get; private set; }

    private readonly List<PriceApplierState> _states = [];
    public IReadOnlyList<PriceApplierState> States => _states;
    
    private PriceApplier() { }

    private PriceApplier(string systemName)
    {
        SystemName = systemName
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("System name cannot be empty"));
    }

    public static PriceApplier Create(
        string systemName,
        string name,
        string dslLogic)
    {
        var applier = new PriceApplier(systemName);
        applier.SetName(name);
        applier.SetDslLogic(dslLogic);
        return applier;
    }

    public static PriceApplier CreateLocal(string systemName)
        => new(systemName);
    
    public void AddState(PriceApplierState state) => _states.Add(state);

    public void RemoveStatesExcept(IEnumerable<PriceOfferSourceType> usages)
    {
        var usagesToKeep = usages.ToHashSet();
        _states.RemoveAll(x => !usagesToKeep.Contains(x.Usage));
    }

    public void SetName(string name)
        => Name = name
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("Name cannot be empty"))
            .EnsureMaxLength(128, () => new InvalidOperationException("Name cannot exceed 128 characters"));

    public void SetDslLogic(string dslLogic)
        => DslLogic = dslLogic
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("DSL logic cannot be empty"))
            .EnsureValidJson(() => new InvalidOperationException("DSL logic is not valid JSON"));
    
    public override string GetId() => SystemName;
    public static Expression<Func<PriceApplier, string>> GetKeySelector()
        => x => x.SystemName;
    public static Expression<Func<PriceApplier, bool>> GetEqualityExpression(string key)
        => x => x.SystemName == key;
}
