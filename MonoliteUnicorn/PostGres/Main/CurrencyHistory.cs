using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class CurrencyHistory
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public decimal PrevValue { get; set; }

    public decimal NewValue { get; set; }

    public DateTime Datetime { get; set; }

    public virtual Currency Currency { get; set; } = null!;
}
