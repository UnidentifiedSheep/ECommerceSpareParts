namespace Core.Entities;

public partial class MarkupRange
{
    public int Id { get; set; }

    public decimal RangeStart { get; set; }

    public decimal RangeEnd { get; set; }

    public decimal Markup { get; set; }

    public int GroupId { get; set; }

    public virtual MarkupGroup Group { get; set; } = null!;
}
