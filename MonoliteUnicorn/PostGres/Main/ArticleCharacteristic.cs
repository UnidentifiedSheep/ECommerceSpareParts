using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class ArticleCharacteristic
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public string Value { get; set; } = null!;

    public string? Name { get; set; }

    public virtual Article Article { get; set; } = null!;
}
