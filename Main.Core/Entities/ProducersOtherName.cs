namespace Main.Core.Entities;

public class ProducersOtherName
{
    public int ProducerId { get; set; }

    public string ProducerOtherName { get; set; } = null!;

    public string WhereUsed { get; set; } = null!;

    public virtual Producer Producer { get; set; } = null!;
}