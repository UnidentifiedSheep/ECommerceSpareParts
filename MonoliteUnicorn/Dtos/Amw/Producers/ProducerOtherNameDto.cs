namespace MonoliteUnicorn.Dtos.Amw.Producers;

public class ProducerOtherNameDto
{
    public int ProducerId { get; set; }
    public string OtherName { get; set; } = null!;
    public string? WhereUsed { get; set; }
}