using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class StorageContentReservation
{
    public string UserId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int InitialCount { get; set; }

    public int CurrentCount { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
