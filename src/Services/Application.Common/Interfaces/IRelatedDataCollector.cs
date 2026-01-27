namespace Application.Common.Interfaces;

public interface IRelatedDataCollector
{
    IDisposable BeginScope();
    void Add(string id);
    void AddRange(IEnumerable<string> ids);
    IReadOnlyCollection<string> CurrentIds { get; }
}