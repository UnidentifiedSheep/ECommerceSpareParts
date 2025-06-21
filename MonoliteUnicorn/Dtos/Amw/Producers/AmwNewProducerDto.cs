namespace MonoliteUnicorn.Dtos.Amw.Producers;

public class AmwNewProducerDto
{
    public string ProducerName { get; set; } = null!;
    public bool IsOe { get; set; } = false;
    public string? Description { get; set; }
}