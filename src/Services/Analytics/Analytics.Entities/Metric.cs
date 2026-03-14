namespace Analytics.Entities;

public partial class Metric
{
    public Guid Id { get; set; }

    public int CurrencyId { get; set; }

    public Guid CreatedBy { get; set; }

    public decimal Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Discriminator { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;
}
