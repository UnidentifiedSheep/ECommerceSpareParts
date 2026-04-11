using BulkValidation.Core.Attributes;

namespace Main.Entities;

public class ProducersOtherName
{
    [ValidateTuple("PK")]
    public int ProducerId { get; set; }

    [ValidateTuple("PK")]
    public string ProducerOtherName { get; set; } = null!;

    [ValidateTuple("PK")]
    public string WhereUsed { get; set; } = null!;
}