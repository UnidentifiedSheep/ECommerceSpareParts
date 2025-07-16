using System;
using System.Collections.Generic;
using MonoliteUnicorn.Enums;

namespace MonoliteUnicorn.PostGres.Main;

public partial class StorageMovement
{
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ArticleId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public string ActionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string WhoMoved { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Storage StorageNameNavigation { get; set; } = null!;

    public virtual AspNetUser WhoMovedNavigation { get; set; } = null!;
    
    public StorageMovement SetActionType(StorageMovementType type)
    {
        ActionType = type.ToString();
        return this;
    }
}
