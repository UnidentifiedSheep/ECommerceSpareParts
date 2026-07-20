using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Pricing.Entities.Pricing;

public class PriceApplier : 
    AuditableEntity<PriceApplier, string>, 
    ILinqEntity<PriceApplier, string>
{
    public string SystemName { get; private set; } = null!;
    public string DslLogic { get; private set; } = null!;

    private readonly List<PriceApplierState> _states = [];
    public IReadOnlyList<PriceApplierState> States => _states;
    
    private PriceApplier() { }

    private PriceApplier(string systemName, string dslLogic)
    {
        SystemName = systemName
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("System name cannot be empty"));
        SetDslLogic(dslLogic);
    }

    public static PriceApplier Create(string systemName, string dslLogic)
        => new(systemName, dslLogic);
    
    public void AddState(PriceApplierState state) => _states.Add(state);

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