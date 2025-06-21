using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class ProducersOtherName
{
    public int ProducerId { get; set; }

    public string ProducerOtherName { get; set; } = null!;

    public string? WhereUsed { get; set; }

    public virtual Producer Producer { get; set; } = null!;
}
