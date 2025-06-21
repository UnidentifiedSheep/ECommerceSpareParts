using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class CurrencyToUsd
{
    public int CurrencyId { get; set; }

    public decimal ToUsd { get; set; }

    public virtual Currency Currency { get; set; } = null!;
}
