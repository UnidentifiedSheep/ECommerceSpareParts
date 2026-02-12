namespace Contracts.Markup;

public record MarkupGroupChangedEvent
{
    public int GroupId { get; init; }
}