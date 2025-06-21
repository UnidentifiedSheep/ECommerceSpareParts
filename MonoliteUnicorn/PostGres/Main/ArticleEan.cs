using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class ArticleEan
{
    public int ArticleId { get; set; }

    public string Ean { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
}
