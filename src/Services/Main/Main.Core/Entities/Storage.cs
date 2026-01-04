namespace Main.Core.Entities;

public partial class Storage
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<StorageRoute> StorageRouteFromStorageNameNavigations { get; set; } = new List<StorageRoute>();

    public virtual ICollection<StorageRoute> StorageRouteToStorageNameNavigations { get; set; } = new List<StorageRoute>();

    public virtual ICollection<User> Owners { get; set; } = new List<User>();
}
