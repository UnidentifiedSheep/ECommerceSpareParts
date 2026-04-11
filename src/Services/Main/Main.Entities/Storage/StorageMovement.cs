using Main.Enums;

namespace Main.Entities.Storage;

public class StorageMovement
{
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ProductId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public StorageMovementType ActionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid WhoMoved { get; set; }

    public StorageMovement SetActionType(StorageMovementType type)
    {
        ActionType = type;
        return this;
    }
}