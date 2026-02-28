using Search.Enums;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IIndexContext
{
    IndexName IndexName { get; }
}