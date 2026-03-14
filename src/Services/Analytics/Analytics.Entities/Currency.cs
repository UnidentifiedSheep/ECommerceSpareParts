namespace Analytics.Entities;

public partial class Currency
{
    public int Id { get; set; }

    public decimal ToUsd { get; set; }

    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();

    public virtual ICollection<PurchasesFact> PurchasesFacts { get; set; } = new List<PurchasesFact>();

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual ICollection<SalesFact> SalesFacts { get; set; } = new List<SalesFact>();
}
