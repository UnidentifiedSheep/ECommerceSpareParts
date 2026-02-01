using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class StorageOwner
{
    [ValidateTuple("PK")]
    [Validate]
    public string StorageName { get; set; } = null!;

    [ValidateTuple("PK")]
    public Guid OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual Storage StorageNameNavigation { get; set; } = null!;
}
