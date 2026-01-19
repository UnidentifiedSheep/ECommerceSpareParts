using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class ProducersOtherName
{
    [ValidateTuple("PK")]
    public int ProducerId { get; set; }

    [ValidateTuple("PK")]
    public string ProducerOtherName { get; set; } = null!;

    [ValidateTuple("PK")]
    public string WhereUsed { get; set; } = null!;

    public virtual Producer Producer { get; set; } = null!;
}
