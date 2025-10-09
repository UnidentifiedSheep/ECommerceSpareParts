using Core.Enums;

namespace Core.Entities;

public class StorageMovement
{
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ArticleId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public string ActionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid WhoMoved { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Storage StorageNameNavigation { get; set; } = null!;

    public virtual User WhoMovedNavigation { get; set; } = null!;

    public StorageMovement SetActionType(StorageMovementType type)
    {
        ActionType = type.ToString();
        return this;
    }
}