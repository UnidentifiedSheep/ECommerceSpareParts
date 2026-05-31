using Analytics.Entities.Interfaces;

namespace Analytics.Application.Models;

public record TagUpdateContext<TEntity>
    where TEntity : IDependency
{
    public DateTime? PreviousFactDatetime { get; init; }
    public required DateTime NewFactDatetime { get; init; }
}