using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class PurchaseContent
{
    public int Id { get; set; }

    public string PurchaseId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal TotalSum { get; set; }

    public string? Comment { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Purchase Purchase { get; set; } = null!;
}
