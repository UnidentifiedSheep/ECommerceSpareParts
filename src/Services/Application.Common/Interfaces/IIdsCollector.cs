namespace Application.Common.Interfaces;

public interface IIdsCollector
{
    IReadOnlyCollection<string> CurrentIds { get; }
    IDisposable BeginScope();
    void Add(string id);
    void AddRange(IEnumerable<string> ids);
}