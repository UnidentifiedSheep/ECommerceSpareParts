namespace Search.Entities;

public class Producer
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public List<ProducerAlias> Aliases { get; init; } = [];
}