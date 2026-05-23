namespace Search.Entities;

public class Producer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public List<ProducerOtherName> OtherNames { get; set; } = [];
}