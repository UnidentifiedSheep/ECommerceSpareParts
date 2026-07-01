using System.Net.Http.Headers;

namespace Integrations.Supplier;

public sealed record ConnectionModel
{
    public required Uri BaseUri { get; init; }

    public AuthenticationHeaderValue? Authorization { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

    public IReadOnlyDictionary<string, string> DefaultQueryParams { get; init; }
        = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> Headers { get; init; }
        = new Dictionary<string, string>();
}