using Analytics.Enums;

namespace Analytics.Entities.Metrics;

public abstract partial class Metric
{
    public Guid Id { get; set; }

    public int CurrencyId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; protected set; }
    
    public DateTime RangeStart { get; protected set; }
    
    public DateTime RangeEnd { get; protected set; }
    
    public DateTime? RecalculatedAt { get; protected set; }

    public string Discriminator { get; protected set; } = null!;
    
    public bool NeedsRecalculation { get; protected set; }
    
    public DependsOn DependsOn { get; protected set; }

    public virtual Currency Currency { get; set; } = null!;
}
