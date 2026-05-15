namespace Abstractions.Interfaces;

public interface ISystemIdExtractor
{
    Task<Guid> ExtractSystemId(CancellationToken cancellationToken = default);
}