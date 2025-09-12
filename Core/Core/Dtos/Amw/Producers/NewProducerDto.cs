namespace Core.Dtos.Amw.Producers;

public class NewProducerDto
{
    public string ProducerName { get; set; } = null!;
    public bool IsOe { get; set; } = false;
    public string? Description { get; set; }
}