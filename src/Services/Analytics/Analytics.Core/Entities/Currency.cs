namespace Analytics.Core.Entities;

public partial class Currency
{
    public int Id { get; set; }

    public decimal ToUsd { get; set; }

    public virtual ICollection<SellInfo> SellInfoBuyCurrencies { get; set; } = new List<SellInfo>();

    public virtual ICollection<SellInfo> SellInfoSellCurrencies { get; set; } = new List<SellInfo>();
}
