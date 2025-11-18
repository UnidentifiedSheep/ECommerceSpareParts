namespace Main.Core.Dtos.Anonymous.Producers;

public class ProducerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsOe { get; set; }
    public string? Description { get; set; }
}