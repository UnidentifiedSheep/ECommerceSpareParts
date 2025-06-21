using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class UserDiscount
{
    public string UserId { get; set; } = null!;

    public decimal? Discount { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
