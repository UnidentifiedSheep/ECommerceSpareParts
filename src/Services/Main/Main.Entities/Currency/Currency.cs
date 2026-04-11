using BulkValidation.Core.Attributes;

namespace Main.Entities.Currency;

public class Currency
{
    [Validate]
    public int Id { get; set; }

    [Validate]
    public string ShortName { get; set; } = null!;

    [Validate]
    public string Name { get; set; } = null!;

    [Validate]
    public string CurrencySign { get; set; } = null!;

    [Validate]
    public string Code { get; set; } = null!;

    public virtual ICollection<CurrencyHistory> CurrencyHistories { get; set; } = new List<CurrencyHistory>();

    public virtual CurrencyToUsd? CurrencyToUsd { get; set; }
}