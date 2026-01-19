using BulkValidation.Core.Attributes;
using Main.Enums;

namespace Main.Entities;

public partial class Storage
{
    [Validate]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public StorageType Type { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<StorageRoute> StorageRouteFromStorageNameNavigations { get; set; } = new List<StorageRoute>();

    public virtual ICollection<StorageRoute> StorageRouteToStorageNameNavigations { get; set; } = new List<StorageRoute>();

    public virtual ICollection<User> Owners { get; set; } = new List<User>();
}
